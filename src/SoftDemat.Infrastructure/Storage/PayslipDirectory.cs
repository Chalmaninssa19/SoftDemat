using Microsoft.Extensions.Options;
using SoftDemat.Domain.Exceptions;
using SoftDemat.Domain.Interfaces;
using SoftDemat.Domain.Rules;
using SoftDemat.Infrastructure.Options;

namespace SoftDemat.Infrastructure.Storage;

public sealed class PayslipDirectory : IPayslipDirectory
{
    private readonly PayslipStorageOptions _options;

    public PayslipDirectory(IOptions<PayslipStorageOptions> options)
    {
        _options = options.Value;
    }

    public IReadOnlyList<string> ListChildDirectories(string? relativeFolder)
    {
        var path = Resolve(relativeFolder);
        return Directory.GetDirectories(path)
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Cast<string>()
            .OrderBy(name => name)
            .ToList();
    }

    public IReadOnlyList<string> ListPdfFileNames(string? relativeFolder)
    {
        var path = Resolve(relativeFolder);
        return Directory.GetFiles(path, "*.pdf")
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Cast<string>()
            .OrderBy(name => name)
            .ToList();
    }

    public string ResolveFile(string? relativeFolder, string fileName)
    {
        var folder = Resolve(relativeFolder);
        var full = Path.GetFullPath(Path.Combine(folder, Path.GetFileName(fileName)));
        EnsureInsideRoot(full);
        return full;
    }

    private string Resolve(string? relativeFolder)
    {
        if (string.IsNullOrWhiteSpace(_options.RootPath))
            throw new DomainException("La racine des bulletins n'est pas configurée.");

        var root = Path.GetFullPath(_options.RootPath);
        var combined = Path.GetFullPath(Path.Combine(root, relativeFolder?.Trim().TrimStart('\\', '/') ?? string.Empty));
        EnsureInsideRoot(combined);
        if (!Directory.Exists(combined))
            throw new DomainException("Dossier introuvable.");

        return combined;
    }

    private void EnsureInsideRoot(string fullPath)
    {
        var root = Path.GetFullPath(_options.RootPath);
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (!fullPath.StartsWith(root, comparison))
            throw new DomainException("Dossier hors de la racine autorisée.");
    }
}
