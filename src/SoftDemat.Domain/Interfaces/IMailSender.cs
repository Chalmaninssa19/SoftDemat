namespace SoftDemat.Domain.Interfaces;

public sealed record OutgoingMail(
    string To,
    string? Bcc,
    string Subject,
    string HtmlBody,
    string AttachmentPath,
    string AttachmentName,
    string? From = null);

public interface IMailSender
{
    Task<string> SendAsync(OutgoingMail mail, CancellationToken cancellationToken = default);
}
