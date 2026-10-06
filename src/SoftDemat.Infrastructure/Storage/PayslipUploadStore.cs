using SoftDemat.Domain.Exceptions;
using SoftDemat.Domain.Rules;

namespace SoftDemat.Infrastructure.Storage;

public sealed class PayslipUploadStore
{
    private readonly PayslipStorageLocation _location;

    public PayslipUploadStore(PayslipStorageLocation location)
    {
        _location = location;
    }

    public async Task<string> SaveAsync(int userId, IReadOnlyList<PayslipUploadWrite> files, CancellationToken cancellationToken)
    {
        PurgeExpired();
        var relative = $"{PayslipUploadPolicy.BatchPrefix}/{userId.ToString(System.Globalization.CultureInfo.InvariantCulture)}/{Guid.NewGuid():N}";
        var directory = CombineInsideRoot(relative);
        Directory.CreateDirectory(directory);
        foreach (var file in files)
            await WriteAsync(directory, file, cancellationToken);

        return relative;
    }

    public void PurgeExpired()
    {
        var root = Path.Combine(_location.Root, PayslipUploadPolicy.BatchPrefix);
        if (!Directory.Exists(root))
            return;

        var limit = DateTime.UtcNow - PayslipUploadPolicy.Retention;
        foreach (var userDirectory in Directory.EnumerateDirectories(root))
            PurgeUser(userDirectory, limit);
    }

    private static void PurgeUser(string userDirectory, DateTime limit)
    {
        foreach (var batch in Directory.EnumerateDirectories(userDirectory))
        {
            if (Directory.GetCreationTimeUtc(batch) < limit)
                Directory.Delete(batch, true);
        }

        if (!Directory.EnumerateFileSystemEntries(userDirectory).Any())
            Directory.Delete(userDirectory);
    }

    private async Task WriteAsync(string directory, PayslipUploadWrite file, CancellationToken cancellationToken)
    {
        if (Path.GetFileName(file.SafeName) != file.SafeName)
            throw new DomainException("Nom de fichier refusé.");

        var full = Path.GetFullPath(Path.Combine(directory, file.SafeName));
        var prefix = Path.GetFullPath(directory).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!full.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            throw new DomainException("Nom de fichier refusé.");

        await using var target = new FileStream(full, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);
        await target.WriteAsync(file.Header, cancellationToken);
        var buffer = new byte[81920];
        long written = file.Header.Length;
        while (true)
        {
            var read = await file.Content.ReadAsync(buffer, cancellationToken);
            if (read == 0)
                return;

            written += read;
            if (written > PayslipUploadPolicy.MaxFileBytes)
                throw new DomainException("Chaque PDF doit faire entre 1 octet et 10 Mo.");

            await target.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
        }
    }

    private string CombineInsideRoot(string relative)
    {
        var full = Path.GetFullPath(Path.Combine(_location.Root, relative.Replace('/', Path.DirectorySeparatorChar)));
        var root = Path.GetFullPath(_location.Root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            throw new DomainException("Dossier hors de la racine autorisée.");

        return full;
    }
}

public sealed record PayslipUploadWrite(string SafeName, byte[] Header, Stream Content);
