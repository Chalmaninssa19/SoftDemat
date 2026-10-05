using System.Globalization;

namespace SoftDemat.Domain.Rules;

public static class ArchivePathBuilder
{
    public static string BuildDirectory(string archiveRoot, DateTime payDate, string? establishmentName)
    {
        var culture = CultureInfo.GetCultureInfo("fr-FR");
        var month = culture.DateTimeFormat.GetMonthName(payDate.Month);
        var establishment = string.IsNullOrWhiteSpace(establishmentName)
            ? "Sans Etablissement"
            : establishmentName.Trim().Replace("/", "-");

        return Path.Combine(archiveRoot, payDate.Year.ToString(culture), month, establishment);
    }

    public static string BuildFileName(string? mailCode, DateTime payDate, string matricule, string? firstName)
    {
        var code = string.IsNullOrWhiteSpace(mailCode) ? "BL" : mailCode.Trim();
        var givenName = string.IsNullOrWhiteSpace(firstName) ? string.Empty : firstName.Trim();
        givenName = givenName.Replace("/", "-").Replace("\\", "-");
        return $"{code}_{payDate:MM_yyyy}_{matricule}_{givenName}.pdf";
    }
}
