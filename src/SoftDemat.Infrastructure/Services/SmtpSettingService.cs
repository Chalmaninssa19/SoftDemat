using Microsoft.Extensions.Logging;
using SoftDemat.Application.DTOs;
using SoftDemat.Application.Services.Interfaces;
using SoftDemat.Domain.Entities;
using SoftDemat.Domain.Exceptions;
using SoftDemat.Domain.Interfaces;

namespace SoftDemat.Infrastructure.Services;

public sealed class SmtpSettingService : ISmtpSettingService
{
    private readonly ISmtpSettingRepository _settings;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SmtpSettingService> _logger;

    public SmtpSettingService(
        ISmtpSettingRepository settings,
        IUnitOfWork unitOfWork,
        ILogger<SmtpSettingService> logger)
    {
        _settings = settings;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IReadOnlyList<SmtpSettingResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _settings.GetAllAsync(cancellationToken);
        return settings.Select(ToResponse).ToList();
    }

    public async Task<SmtpSettingResponse> CreateAsync(
        UpdateSmtpSettingRequest request,
        CancellationToken cancellationToken = default)
    {
        var active = await _settings.GetActiveAsync(cancellationToken);
        var setting = new SmtpSetting { IsActive = active is null };
        Apply(setting, request);
        await _settings.AddAsync(setting, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Configuration SMTP {Name} créée", setting.Name);
        return ToResponse(setting);
    }

    public async Task<SmtpSettingResponse> UpdateAsync(
        int id,
        UpdateSmtpSettingRequest request,
        CancellationToken cancellationToken = default)
    {
        var setting = await GetRequiredAsync(id, cancellationToken);
        Apply(setting, request);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Configuration SMTP {Name} mise à jour", setting.Name);
        return ToResponse(setting);
    }

    public async Task ActivateAsync(int id, CancellationToken cancellationToken = default)
    {
        var setting = await GetRequiredAsync(id, cancellationToken);
        var settings = await _settings.GetAllForUpdateAsync(cancellationToken);
        foreach (var current in settings)
            current.IsActive = current.Id == setting.Id;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Configuration SMTP {Name} activée", setting.Name);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var setting = await GetRequiredAsync(id, cancellationToken);
        var settings = await _settings.GetAllForUpdateAsync(cancellationToken);
        if (setting.IsActive)
        {
            var replacement = settings.Where(current => current.Id != id).OrderBy(current => current.Id).FirstOrDefault();
            if (replacement is not null)
                replacement.IsActive = true;
        }

        _settings.Remove(setting);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Configuration SMTP {Name} supprimée", setting.Name);
    }

    private async Task<SmtpSetting> GetRequiredAsync(int id, CancellationToken cancellationToken)
        => await _settings.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Configuration SMTP introuvable.");

    private static void Apply(SmtpSetting setting, UpdateSmtpSettingRequest request)
    {
        setting.Name = request.Name.Trim();
        setting.Host = request.Host.Trim();
        setting.Port = request.Port;
        setting.UseSsl = request.UseSsl;
        setting.UserName = request.User?.Trim() ?? string.Empty;
        setting.FromAddress = request.FromAddress.Trim();
        if (!string.IsNullOrEmpty(request.Password))
            setting.Password = request.Password;
    }

    private static SmtpSettingResponse ToResponse(SmtpSetting setting)
        => new(
            setting.Id,
            setting.Name,
            setting.IsActive,
            setting.Host,
            setting.Port,
            setting.UseSsl,
            setting.UserName,
            setting.Password.Length > 0,
            setting.FromAddress);
}
