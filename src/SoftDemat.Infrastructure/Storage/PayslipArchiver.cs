using SoftDemat.Domain.Exceptions;
using SoftDemat.Domain.Interfaces;
using SoftDemat.Domain.Rules;

namespace SoftDemat.Infrastructure.Storage;

public sealed class PayslipArchiver : IPayslipArchiver
{
    public void Archive(
        string sourceFile,
        string archiveRoot,
        DateTime payDate,
        string? establishmentName,
        string? mailCode,
        string matricule,
        string? firstName)
    {
        if (string.IsNullOrWhiteSpace(archiveRoot))
            throw new DomainException("Le dossier d'archivage n'est pas configuré.");

        var directory = ArchivePathBuilder.BuildDirectory(archiveRoot, payDate, establishmentName);
        Directory.CreateDirectory(directory);
        var target = Path.GetFullPath(Path.Combine(directory, ArchivePathBuilder.BuildFileName(mailCode, payDate, matricule, firstName)));
        var root = Path.GetFullPath(archiveRoot);
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (!target.StartsWith(root, comparison))
            throw new DomainException("Archivage hors du dossier autorisé.");

        File.Copy(sourceFile, target, overwrite: true);
    }
}
