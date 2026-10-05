namespace SoftDemat.Domain.Rules;

public readonly record struct ParsedPayslipFile(string Matricule, DateTime PayDate, string FileName);

public static class PayslipFileNameParser
{
    public static bool TryParse(string? fileName, out ParsedPayslipFile parsed)
    {
        parsed = default;
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        var name = Path.GetFileName(fileName).ToLowerInvariant();
        if (!name.EndsWith(".pdf", StringComparison.Ordinal))
            return false;

        var stem = name[..^4];
        var parts = stem.Split('_');
        if (parts.Length < 2 || parts[1].Length < 8)
            return false;

        var matricule = parts[0].Length < 4 ? parts[0].PadLeft(4) : parts[0];
        var rawDate = parts[1][..8];
        if (!int.TryParse(rawDate[..4], out var year)
            || !int.TryParse(rawDate.Substring(4, 2), out var month)
            || !int.TryParse(rawDate.Substring(6, 2), out var day))
            return false;

        try
        {
            var payDate = new DateTime(year, month, day);
            parsed = new ParsedPayslipFile(matricule, payDate, Path.GetFileName(fileName));
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }
}
