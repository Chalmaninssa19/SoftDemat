namespace SoftDemat.Domain.Rules;

public static class MailSenderTools
{
    public const string Outlook = "Outlook";
    public const string Address = "Address";

    public static bool IsKnown(string? tool)
        => tool is Outlook or Address;

    public static bool IsOutlook(string? tool)
        => tool == Outlook;
}
