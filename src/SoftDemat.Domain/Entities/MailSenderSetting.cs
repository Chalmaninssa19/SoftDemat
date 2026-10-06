using SoftDemat.Domain.Rules;

namespace SoftDemat.Domain.Entities;

public sealed class MailSenderSetting : BaseEntity
{
    public const int SingletonId = 1;

    public string SenderTool { get; set; } = MailSenderTools.Outlook;
    public string SenderAddress { get; set; } = string.Empty;
}
