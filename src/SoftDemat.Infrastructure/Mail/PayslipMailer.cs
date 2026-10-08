using Microsoft.Extensions.Logging;
using SoftDemat.Domain.Exceptions;
using SoftDemat.Domain.Interfaces;
using SoftDemat.Domain.Rules;

namespace SoftDemat.Infrastructure.Mail;

public sealed class PayslipMailer : IPayslipMailer
{
    private readonly IMailSenderSettingRepository _settings;
    private readonly IMailSender _mailKit;
    private readonly ILocalMailbox _outlook;
    private readonly ILogger<PayslipMailer> _logger;

    public PayslipMailer(
        IMailSenderSettingRepository settings,
        IMailSender mailKit,
        ILocalMailbox outlook,
        ILogger<PayslipMailer> logger)
    {
        _settings = settings;
        _mailKit = mailKit;
        _outlook = outlook;
        _logger = logger;
    }

    public async Task<string> SendAsync(OutgoingMail mail, bool onSenderMachine, CancellationToken cancellationToken = default)
    {
        var setting = await _settings.GetAsync(cancellationToken);
        var tool = setting is null ? MailSenderTools.Outlook : MailSenderTools.Resolve(setting.SenderTool);
        if (tool is null)
            throw new DomainException("Le mode d'envoi sélectionné n'est pas reconnu.");

        _logger.LogInformation("Envoi d'un bulletin par {Mode}", tool);
        return tool == MailSenderTools.Outlook
            ? SendWithOutlook(mail, onSenderMachine)
            : await _mailKit.SendAsync(mail, cancellationToken);
    }

    private string SendWithOutlook(OutgoingMail mail, bool onSenderMachine)
    {
        if (!onSenderMachine)
            throw new DomainException("Outlook s'utilise sur le poste de la personne qui clique sur Envoyer. Lancez SoftDemat sur ce PC, ou choisissez l'envoi MailKit.");

        return _outlook.Send(mail);
    }
}
