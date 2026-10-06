using SoftDemat.Domain.Exceptions;
using SoftDemat.Domain.Interfaces;
using SoftDemat.Infrastructure.Mail;

namespace SoftDemat.Infrastructure.Storage;

public sealed class WindowsArchiveFolderPicker : IArchiveFolderPicker
{
    public string? Pick()
        => StaRunner.Run(PickCore);

    private static string? PickCore()
    {
        var type = Type.GetTypeFromProgID("Shell.Application");
        if (type is null)
            throw new DomainException("Le choix de dossier n'est pas disponible sur ce poste.");

        dynamic shell = Activator.CreateInstance(type)!;
        dynamic folder = shell.BrowseForFolder(0, "Dossier d'archivage des bulletins", 0x41, 0);
        if (folder is null)
            return null;

        var path = (string?)folder.Self.Path;
        return string.IsNullOrWhiteSpace(path) ? null : path.Trim();
    }
}
