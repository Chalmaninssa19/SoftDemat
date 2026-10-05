using Microsoft.Extensions.Logging;
using SoftDemat.Application.DTOs;
using SoftDemat.Application.Services.Interfaces;
using SoftDemat.Domain.Entities;
using SoftDemat.Domain.Exceptions;
using SoftDemat.Domain.Interfaces;

namespace SoftDemat.Infrastructure.Services;

public sealed class SageConnectionService : ISageConnectionService
{
    private readonly ISageConnectionRepository _connections;
    private readonly ISageConnectionTester _tester;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SageConnectionService> _logger;

    public SageConnectionService(
        ISageConnectionRepository connections,
        ISageConnectionTester tester,
        IUnitOfWork unitOfWork,
        ILogger<SageConnectionService> logger)
    {
        _connections = connections;
        _tester = tester;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<SageConnectionResponse> GetAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _connections.GetAsync(cancellationToken);
        return settings is null
            ? new SageConnectionResponse(string.Empty, string.Empty, string.Empty, false)
            : ToResponse(settings);
    }

    public async Task<SageConnectionResponse> UpdateAsync(
        UpdateSageConnectionRequest request,
        CancellationToken cancellationToken = default)
    {
        var settings = await _connections.GetAsync(cancellationToken) ?? new SageConnectionSettings();
        Apply(settings, request);
        if (settings.Id == 0)
            await _connections.AddAsync(settings, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Connexion Sage enregistrée pour {Database}", settings.DatabaseName);
        return ToResponse(settings);
    }

    public async Task TestAsync(UpdateSageConnectionRequest request, CancellationToken cancellationToken = default)
    {
        var settings = await _connections.GetAsync(cancellationToken);
        var login = request.WindowsAuthentication ? string.Empty : request.Login;
        var password = string.IsNullOrEmpty(request.Password) ? settings?.Password : request.Password;
        await _tester.TestAsync(request.Server, request.DatabaseName, login, password, cancellationToken);
    }

    private static void Apply(SageConnectionSettings settings, UpdateSageConnectionRequest request)
    {
        settings.Server = request.Server.Trim();
        settings.DatabaseName = request.DatabaseName.Trim();
        settings.AuthenticationType = true;
        settings.Login = request.WindowsAuthentication ? string.Empty : request.Login?.Trim() ?? string.Empty;
        if (!string.IsNullOrEmpty(request.Password))
            settings.Password = request.Password;
        if (request.WindowsAuthentication)
            settings.Password = string.Empty;
    }

    private static SageConnectionResponse ToResponse(SageConnectionSettings settings)
        => new(settings.Server, settings.DatabaseName, settings.Login, !string.IsNullOrEmpty(settings.Password));
}
