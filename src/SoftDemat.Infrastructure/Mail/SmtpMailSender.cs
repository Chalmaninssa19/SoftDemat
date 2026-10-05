using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using SoftDemat.Domain.Exceptions;
using SoftDemat.Domain.Interfaces;
using SoftDemat.Infrastructure.Options;

namespace SoftDemat.Infrastructure.Mail;

public sealed class SmtpMailSender : IMailSender
{
    private readonly SmtpOptions _options;

    public SmtpMailSender(IOptions<SmtpOptions> options)
    {
        _options = options.Value;
    }

    public async Task SendAsync(OutgoingMail mail, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.Host) || string.IsNullOrWhiteSpace(_options.From))
            throw new DomainException("Le serveur SMTP n'est pas configuré.");

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(_options.From));
        message.To.Add(MailboxAddress.Parse(mail.To));
        if (!string.IsNullOrWhiteSpace(mail.Bcc))
            message.Bcc.Add(MailboxAddress.Parse(mail.Bcc));

        message.Subject = mail.Subject;
        var builder = new BodyBuilder { HtmlBody = mail.HtmlBody };
        var attachment = await builder.Attachments.AddAsync(mail.AttachmentPath, cancellationToken);
        if (attachment.ContentDisposition is not null)
            attachment.ContentDisposition.FileName = mail.AttachmentName;

        message.Body = builder.ToMessageBody();
        using var client = new SmtpClient();
        var socket = _options.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
        await client.ConnectAsync(_options.Host, _options.Port, socket, cancellationToken);
        if (!string.IsNullOrWhiteSpace(_options.User))
            await client.AuthenticateAsync(_options.User, _options.Password, cancellationToken);

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
