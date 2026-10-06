using SoftDemat.Domain.Exceptions;
using SoftDemat.Domain.Interfaces;

namespace SoftDemat.Infrastructure.Storage;

public sealed class PayslipDirectory : IPayslipDirectory
{
    private readonly PayslipStorageLocation _location;
    private readonly PayslipUploadStore _uploads;

    public PayslipDirectory(PayslipStorageLocation location, PayslipUploadStore uploads)
    {
        _location = location;
        _uploads = uploads;
    }

    public IReadOnlyList<string> ListChildDirectories(string? relativeFolder)
    {
        var path = Resolve(relativeFolder);
        return Directory.GetDirectories(path)
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Cast<string>()
            .OrderBy(name => name)
            .ToList();
    }

    public IReadOnlyList<string> ListPdfFileNames(string? relativeFolder)
    {
        var path = Resolve(relativeFolder);
        return Directory.GetFiles(path, "*.pdf")
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Cast<string>()
            .OrderBy(name => name)
            .ToList();
    }

    public string ResolveFile(string? relativeFolder, string fileName)
    {
        var folder = Resolve(relativeFolder);
        var full = Path.GetFullPath(Path.Combine(folder, Path.GetFileName(fileName)));
        EnsureInsideRoot(full);
        return full;
    }

    private string Resolve(string? relativeFolder)
    {
        _uploads.PurgeExpired();
        var relative = relativeFolder?.Trim().TrimStart('\\', '/') ?? string.Empty;
        var combined = Path.GetFullPath(Path.Combine(_location.Root, relative));
        EnsureInsideRoot(combined);
        if (!Directory.Exists(combined))
            throw new DomainException("Le dossier téléversé est introuvable ou a expiré.");

        return combined;
    }

    private void EnsureInsideRoot(string fullPath)
    {
        var root = Path.GetFullPath(_location.Root).TrimEnd(Path.DirectorySeparatorChar);
        var prefix = root + Path.DirectorySeparatorChar;
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        var inside = fullPath.Equals(root, comparison) || fullPath.StartsWith(prefix, comparison);
        if (!inside)
            throw new DomainException("Dossier hors de la racine autorisée.");
    }
}
