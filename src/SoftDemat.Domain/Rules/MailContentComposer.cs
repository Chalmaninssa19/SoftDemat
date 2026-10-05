using System.Globalization;

namespace SoftDemat.Domain.Rules;

public static class MailContentComposer
{
    public static string Apply(
        string? template,
        DateTime payDate,
        string? firstName,
        string? fullName,
        string? matricule)
    {
        var culture = CultureInfo.GetCultureInfo("fr-FR");
        var content = template ?? string.Empty;
        content = content.Replace("MM", culture.DateTimeFormat.GetMonthName(payDate.Month));
        content = content.Replace("AA", payDate.Year.ToString(culture));
        content = content.Replace("PNOM", firstName ?? string.Empty);
        content = content.Replace("NOM", fullName ?? string.Empty);
        content = content.Replace("MAT", matricule ?? string.Empty);
        return content.Replace("\n", "<br/>");
    }
}
