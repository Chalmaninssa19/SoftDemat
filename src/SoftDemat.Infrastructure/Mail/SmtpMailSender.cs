using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using SoftDemat.Domain.Entities;
using SoftDemat.Domain.Exceptions;
using SoftDemat.Domain.Interfaces;

namespace SoftDemat.Infrastructure.Mail;

public sealed class SmtpMailSender : IMailSender
{
    private readonly ISmtpSettingRepository _settings;

    public SmtpMailSender(ISmtpSettingRepository settings)
    {
        _settings = settings;
    }

    public async Task<string> SendAsync(OutgoingMail mail, CancellationToken cancellationToken = default)
    {
        var setting = await _settings.GetAsync(cancellationToken);
        var from = ResolveFrom(mail, setting);
        var message = await CreateMessageAsync(mail, from, cancellationToken);
        await DeliverAsync(setting!, message, cancellationToken);
        return from;
    }

    private static string ResolveFrom(OutgoingMail mail, SmtpSetting? setting)
    {
        var from = string.IsNullOrWhiteSpace(mail.From) ? setting?.FromAddress : mail.From;
        if (setting is null || string.IsNullOrWhiteSpace(setting.Host) || string.IsNullOrWhiteSpace(from))
            throw new DomainException("Le serveur SMTP n'est pas configuré.");

        return from;
    }

    private static async Task<MimeMessage> CreateMessageAsync(OutgoingMail mail, string from, CancellationToken cancellationToken)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(from));
        message.To.Add(MailboxAddress.Parse(mail.To));
        if (!string.IsNullOrWhiteSpace(mail.Bcc))
            message.Bcc.Add(MailboxAddress.Parse(mail.Bcc));

        message.Subject = mail.Subject;
        var builder = new BodyBuilder { HtmlBody = mail.HtmlBody };
        var attachment = await builder.Attachments.AddAsync(mail.AttachmentPath, cancellationToken);
        if (attachment.ContentDisposition is not null)
            attachment.ContentDisposition.FileName = mail.AttachmentName;

        message.Body = builder.ToMessageBody();
        return message;
    }

    private static async Task DeliverAsync(SmtpSetting setting, MimeMessage message, CancellationToken cancellationToken)
    {
        using var client = new SmtpClient();
        var socket = setting.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
        await client.ConnectAsync(setting.Host, setting.Port, socket, cancellationToken);
        if (!string.IsNullOrWhiteSpace(setting.UserName))
            await client.AuthenticateAsync(setting.UserName, setting.Password, cancellationToken);

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
