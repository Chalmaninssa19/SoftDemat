namespace SoftDemat.Domain.Entities;

public sealed class MailTemplate : BaseEntity
{
    public string MailType { get; set; } = string.Empty;
    public string MailObject { get; set; } = string.Empty;
    public string MailContent { get; set; } = string.Empty;
    public string MailCode { get; set; } = string.Empty;
}
