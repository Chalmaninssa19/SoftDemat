namespace SoftDemat.Infrastructure.Options;

public sealed class LegacyEncryptionOptions
{
    public const string SectionName = "LegacyEncryption";
    public string Passphrase { get; set; } = string.Empty;
    public string InitVector { get; set; } = string.Empty;
}
