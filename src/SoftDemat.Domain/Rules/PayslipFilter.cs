namespace SoftDemat.Domain.Rules;

public static class PayslipFilter
{
    public static bool Matches(
        string matricule,
        string fullName,
        string establishmentCode,
        string? establishmentFilter,
        string? employeeMatricule,
        string? matriculeFilter,
        string? nameFilter)
    {
        if (!ContainsIgnoreCase(establishmentCode, establishmentFilter, exact: true))
            return false;
        if (!ContainsIgnoreCase(matricule, employeeMatricule, exact: true))
            return false;
        if (!ContainsIgnoreCase(matricule, matriculeFilter, exact: false))
            return false;
        if (!ContainsIgnoreCase(fullName, nameFilter, exact: false))
            return false;

        return true;
    }

    private static bool ContainsIgnoreCase(string value, string? filter, bool exact)
    {
        if (string.IsNullOrWhiteSpace(filter))
            return true;

        var left = value.Trim();
        var right = filter.Trim();
        if (exact)
            return left.Equals(right, StringComparison.OrdinalIgnoreCase);

        return left.Contains(right, StringComparison.OrdinalIgnoreCase);
    }
}
