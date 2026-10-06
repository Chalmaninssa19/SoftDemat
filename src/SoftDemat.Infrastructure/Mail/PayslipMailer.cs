using SoftDemat.Domain.Entities;
using SoftDemat.Domain.Exceptions;
using SoftDemat.Domain.Interfaces;
using SoftDemat.Domain.Rules;

namespace SoftDemat.Infrastructure.Mail;

public sealed class PayslipMailer : IPayslipMailer
{
    private readonly IMailSenderSettingRepository _settings;
    private readonly IMailSender _smtp;
    private readonly ILocalMailbox _outlook;

    public PayslipMailer(IMailSenderSettingRepository settings, IMailSender smtp, ILocalMailbox outlook)
    {
        _settings = settings;
        _smtp = smtp;
        _outlook = outlook;
    }

    public async Task<string> SendAsync(OutgoingMail mail, bool onSenderMachine, CancellationToken cancellationToken = default)
    {
        var setting = await _settings.GetAsync(cancellationToken);
        if (MailSenderTools.IsOutlook(setting?.SenderTool ?? MailSenderTools.Outlook))
            return SendWithOutlook(mail, onSenderMachine);

        var from = setting?.SenderAddress?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(from))
            throw new DomainException("L'adresse de l'expéditeur n'est pas configurée.");

        await _smtp.SendAsync(mail with { From = from }, cancellationToken);
        return from;
    }

    private string SendWithOutlook(OutgoingMail mail, bool onSenderMachine)
    {
        if (!onSenderMachine)
            throw new DomainException("Outlook s'utilise sur le poste de la personne qui clique sur Envoyer. Lancez SoftDemat sur ce PC, ou choisissez une adresse d'expéditeur.");

        return _outlook.Send(mail);
    }
}
