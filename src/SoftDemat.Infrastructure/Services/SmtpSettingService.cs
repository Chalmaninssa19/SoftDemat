using Microsoft.Extensions.Logging;
using SoftDemat.Application.DTOs;
using SoftDemat.Application.Services.Interfaces;
using SoftDemat.Domain.Entities;
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

    public async Task<SmtpSettingResponse> GetAsync(CancellationToken cancellationToken = default)
    {
        var setting = await _settings.GetAsync(cancellationToken);
        return setting is null ? Empty() : ToResponse(setting);
    }

    public async Task<SmtpSettingResponse> UpdateAsync(
        UpdateSmtpSettingRequest request,
        CancellationToken cancellationToken = default)
    {
        var setting = await _settings.GetAsync(cancellationToken);
        var creating = setting is null;
        setting ??= new SmtpSetting { Id = SmtpSetting.SingletonId };
        Apply(setting, request);
        if (creating)
            await _settings.AddAsync(setting, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Paramètres SMTP enregistrés pour {Host}", setting.Host);
        return ToResponse(setting);
    }

    private static void Apply(SmtpSetting setting, UpdateSmtpSettingRequest request)
    {
        setting.Host = request.Host.Trim();
        setting.Port = request.Port;
        setting.UseSsl = request.UseSsl;
        setting.UserName = request.User?.Trim() ?? string.Empty;
        setting.FromAddress = request.FromAddress.Trim();
        if (!string.IsNullOrEmpty(request.Password))
            setting.Password = request.Password;
    }

    private static SmtpSettingResponse Empty()
        => new(string.Empty, 587, true, string.Empty, false, string.Empty);

    private static SmtpSettingResponse ToResponse(SmtpSetting setting)
        => new(setting.Host, setting.Port, setting.UseSsl, setting.UserName, setting.Password.Length > 0, setting.FromAddress);
}
