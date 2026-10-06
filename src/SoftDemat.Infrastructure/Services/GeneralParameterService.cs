using Microsoft.Extensions.Logging;
using SoftDemat.Application.DTOs;
using SoftDemat.Application.Services.Interfaces;
using SoftDemat.Domain.Entities;
using SoftDemat.Domain.Exceptions;
using SoftDemat.Domain.Interfaces;
using SoftDemat.Domain.Rules;

namespace SoftDemat.Infrastructure.Services;

public sealed class GeneralParameterService : IGeneralParameterService
{
    private readonly IGeneralParameterRepository _parameters;
    private readonly IMailSenderSettingRepository _senders;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GeneralParameterService> _logger;

    public GeneralParameterService(
        IGeneralParameterRepository parameters,
        IMailSenderSettingRepository senders,
        IUnitOfWork unitOfWork,
        ILogger<GeneralParameterService> logger)
    {
        _parameters = parameters;
        _senders = senders;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<GeneralParameterResponse> GetAsync(CancellationToken cancellationToken = default)
    {
        var parameter = await GetRequiredAsync(cancellationToken);
        var sender = await _senders.GetAsync(cancellationToken);
        return ToResponse(parameter, sender);
    }

    public async Task<GeneralParameterResponse> UpdateAsync(
        UpdateGeneralParameterRequest request,
        CancellationToken cancellationToken = default)
    {
        var parameter = await GetRequiredAsync(cancellationToken);
        parameter.ArchiveFolder = request.ArchiveFolder.Trim();
        parameter.Cc = request.Cc?.Trim() ?? string.Empty;
        var sender = await SaveSenderAsync(request, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Paramètres d'envoi mis à jour");
        return ToResponse(parameter, sender);
    }

    private async Task<MailSenderSetting> SaveSenderAsync(UpdateGeneralParameterRequest request, CancellationToken cancellationToken)
    {
        var sender = await _senders.GetAsync(cancellationToken);
        var creating = sender is null;
        sender ??= new MailSenderSetting { Id = MailSenderSetting.SingletonId };
        sender.SenderTool = request.SenderTool;
        sender.SenderAddress = MailSenderTools.IsOutlook(request.SenderTool) ? string.Empty : request.SenderAddress!.Trim();
        if (creating)
            await _senders.AddAsync(sender, cancellationToken);
        return sender;
    }

    private async Task<GeneralParameter> GetRequiredAsync(CancellationToken cancellationToken)
        => await _parameters.GetAsync(cancellationToken)
            ?? throw new NotFoundException("Paramètres généraux introuvables.");

    private static GeneralParameterResponse ToResponse(GeneralParameter parameter, MailSenderSetting? sender)
        => new(
            parameter.ArchiveFolder,
            parameter.Cc,
            sender?.SenderTool ?? MailSenderTools.Outlook,
            sender?.SenderAddress ?? string.Empty);
}
