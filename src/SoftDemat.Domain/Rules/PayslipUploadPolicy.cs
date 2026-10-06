using System.Globalization;
using SoftDemat.Domain.Exceptions;

namespace SoftDemat.Domain.Rules;

public static class PayslipUploadPolicy
{
    public const int MaxFiles = 200;
    public const long MaxFileBytes = 10 * 1024 * 1024;
    public const long MaxBatchBytes = 200L * 1024 * 1024;
    public const long MaxTransportBytes = MaxBatchBytes + (1024 * 1024);
    public const string BatchPrefix = "batches";
    public static readonly TimeSpan Retention = TimeSpan.FromHours(24);

    public static bool TryAccept(
        string? browserPath,
        long length,
        ReadOnlySpan<byte> header,
        ISet<string> acceptedNames,
        out string safeName,
        out string error)
    {
        safeName = string.Empty;
        if (!TryDirectPdfName(browserPath, out safeName, out error) || !TrySize(length, out error))
            return false;
        if (!IsBulletin(safeName, header))
            return RejectContent(header, out error);

        if (acceptedNames.Add(safeName))
        {
            error = string.Empty;
            return true;
        }

        error = "Un bulletin porte déjà ce nom.";
        return false;
    }

    private static bool TrySize(long length, out string error)
    {
        var allowed = length > 0 && length <= MaxFileBytes;
        error = allowed ? string.Empty : "Chaque PDF doit faire entre 1 octet et 10 Mo.";
        return allowed;
    }

    private static bool RejectContent(ReadOnlySpan<byte> header, out string error)
    {
        error = HasPdfHeader(header)
            ? "Nom non reconnu. Attendu : matricule_aaaammjj.pdf."
            : "Le fichier n'est pas un PDF.";
        return false;
    }

    private static bool IsBulletin(string safeName, ReadOnlySpan<byte> header)
        => HasPdfHeader(header) && PayslipFileNameParser.TryParse(safeName, out _);

    public static bool HasPdfHeader(ReadOnlySpan<byte> header)
        => header.Length >= 5
            && header[0] == (byte)'%'
            && header[1] == (byte)'P'
            && header[2] == (byte)'D'
            && header[3] == (byte)'F'
            && header[4] == (byte)'-';

    public static void EnsureOwned(string? relativeFolder, int userId)
    {
        if (string.IsNullOrWhiteSpace(relativeFolder))
            throw new DomainException("Sélectionnez un dossier de bulletins.");

        if (!IsOwned(relativeFolder, userId))
            throw new ForbiddenException("Ce dossier de bulletins ne vous appartient pas.");
    }

    public static bool IsOwned(string? relativeFolder, int userId)
    {
        var parts = Segments(relativeFolder);
        return userId > 0
            && parts.Length == 3
            && parts[0] == BatchPrefix
            && parts[1] == userId.ToString(CultureInfo.InvariantCulture)
            && Guid.TryParseExact(parts[2], "N", out _);
    }

    public static string FolderLabel(string? browserPath)
    {
        var parts = Segments(browserPath);
        if (parts.Length == 0)
            return "Dossier";

        var label = parts[0].Length > 80 ? parts[0][..80] : parts[0];
        return label.Replace('\0', ' ').Trim();
    }

    public static string DisplayName(string? browserPath)
    {
        var parts = Segments(browserPath);
        var name = parts.Length == 0 ? "fichier" : parts[^1];
        return name.Length > 80 ? name[..80] : name;
    }

    private static bool TryDirectPdfName(string? browserPath, out string safeName, out string error)
    {
        safeName = string.Empty;
        error = "Sélectionnez un dossier, pas un fichier isolé.";
        if (string.IsNullOrWhiteSpace(browserPath) || browserPath.Contains('\0') || browserPath.Contains(':'))
            return false;

        var parts = Segments(browserPath);
        if (parts.Any(part => part == ".."))
        {
            error = "Chemin de fichier refusé.";
            return false;
        }

        if (parts.Length != 2)
        {
            error = parts.Length > 2
                ? "Seul le dossier sélectionné est lu, pas ses sous-dossiers."
                : error;
            return false;
        }

        safeName = parts[1];
        error = IsSafePdfName(safeName) ? string.Empty : "Seuls les fichiers PDF du dossier sont acceptés.";
        return error.Length == 0;
    }

    private static bool IsSafePdfName(string name)
    {
        if (name.Length is < 6 or > 120 || !name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            return false;

        foreach (var character in name)
        {
            var allowed = character is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or (>= '0' and <= '9') or '_' or '-' or '.';
            if (!allowed)
                return false;
        }

        return !name.Contains("..", StringComparison.Ordinal);
    }

    private static string[] Segments(string? path)
        => string.IsNullOrWhiteSpace(path)
            ? []
            : path.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
