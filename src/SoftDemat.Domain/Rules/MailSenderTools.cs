namespace SoftDemat.Domain.Rules;

public static class MailSenderTools
{
    public const string Outlook = "Outlook";
    public const string MailKit = "MailKit";
    public const string LegacyAddress = "Address";

    public static bool IsKnown(string? tool)
        => tool is Outlook or MailKit;

    public static bool IsOutlook(string? tool)
        => tool == Outlook;

    public static bool IsMailKit(string? tool)
        => tool == MailKit;

    public static string? Resolve(string? stored)
    {
        if (stored == Outlook)
            return Outlook;
        if (stored is MailKit or LegacyAddress)
            return MailKit;
        return null;
    }
}
