using Microsoft.Extensions.Logging;
using SoftDemat.Application.DTOs;
using SoftDemat.Application.Services.Interfaces;
using SoftDemat.Domain.Exceptions;
using SoftDemat.Domain.Rules;
using SoftDemat.Infrastructure.Storage;

namespace SoftDemat.Infrastructure.Services;

public sealed class PayslipUploadService : IPayslipUploadService
{
    private readonly PayslipUploadStore _store;
    private readonly ILogger<PayslipUploadService> _logger;

    public PayslipUploadService(PayslipUploadStore store, ILogger<PayslipUploadService> logger)
    {
        _store = store;
        _logger = logger;
    }

    public async Task<PayslipUploadResponse> ImportAsync(
        int userId,
        IReadOnlyList<PayslipUploadItem> files,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await ImportCoreAsync(userId, files, cancellationToken);
        }
        finally
        {
            foreach (var file in files)
                await file.Content.DisposeAsync();
        }
    }

    private async Task<PayslipUploadResponse> ImportCoreAsync(
        int userId,
        IReadOnlyList<PayslipUploadItem> files,
        CancellationToken cancellationToken)
    {
        if (userId <= 0)
            throw new ForbiddenException("Authentification requise.");

        EnsureBatchLimits(files);
        var accepted = new List<PayslipUploadWrite>();
        var rejected = new List<PayslipUploadRejection>();
        await CollectAsync(files, accepted, rejected, cancellationToken);
        if (accepted.Count == 0)
            return new PayslipUploadResponse(string.Empty, string.Empty, [], rejected);

        var relative = await _store.SaveAsync(userId, accepted, cancellationToken);
        _logger.LogInformation(
            "Lot {Batch} téléversé pour l'utilisateur {UserId} : {Count} PDF",
            relative,
            userId,
            accepted.Count);
        return Created(files, accepted, rejected, relative);
    }

    private static PayslipUploadResponse Created(
        IReadOnlyList<PayslipUploadItem> files,
        List<PayslipUploadWrite> accepted,
        List<PayslipUploadRejection> rejected,
        string relative)
        => new(
            relative,
            PayslipUploadPolicy.FolderLabel(files[0].BrowserPath),
            accepted.Select(file => file.SafeName).ToList(),
            rejected);

    private static void EnsureBatchLimits(IReadOnlyList<PayslipUploadItem> files)
    {
        if (files.Count == 0)
            throw new DomainException("Sélectionnez un dossier contenant des PDF.");
        if (files.Count > PayslipUploadPolicy.MaxFiles)
            throw new DomainException("200 fichiers maximum par dossier.");
        if (files.Sum(file => Math.Max(file.Length, 0)) > PayslipUploadPolicy.MaxBatchBytes)
            throw new DomainException("Le dossier dépasse 200 Mo.");
    }

    private static async Task CollectAsync(
        IReadOnlyList<PayslipUploadItem> files,
        List<PayslipUploadWrite> accepted,
        List<PayslipUploadRejection> rejected,
        CancellationToken cancellationToken)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in files)
        {
            var header = await ReadHeaderAsync(file.Content, cancellationToken);
            if (!PayslipUploadPolicy.TryAccept(file.BrowserPath, file.Length, header, names, out var safeName, out var error))
            {
                rejected.Add(new PayslipUploadRejection(PayslipUploadPolicy.DisplayName(file.BrowserPath), error));
                continue;
            }

            accepted.Add(new PayslipUploadWrite(safeName, header, file.Content));
        }
    }

    private static async Task<byte[]> ReadHeaderAsync(Stream content, CancellationToken cancellationToken)
    {
        var header = new byte[5];
        var read = await content.ReadAsync(header.AsMemory(0, header.Length), cancellationToken);
        return header[..read];
    }
}
