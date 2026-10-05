using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using SoftDemat.Domain.Interfaces;
using SoftDemat.Infrastructure.Options;

namespace SoftDemat.Infrastructure.Security;

public sealed class LegacyPasswordProtector : ILegacyPasswordProtector
{
    private readonly LegacyEncryptionOptions _options;

    public LegacyPasswordProtector(IOptions<LegacyEncryptionOptions> options)
    {
        _options = options.Value;
    }

    public bool CanUse
        => !string.IsNullOrWhiteSpace(_options.Passphrase) && !string.IsNullOrWhiteSpace(_options.InitVector);

    public string Encrypt(string plainText) => Convert.ToBase64String(Transform(plainText, encrypt: true));

    public string Decrypt(string cipherText)
    {
        var plain = Transform(cipherText, encrypt: false);
        return Encoding.UTF8.GetString(plain);
    }

    private byte[] Transform(string value, bool encrypt)
    {
        var key = DeriveKey(_options.Passphrase);
        var vector = Encoding.ASCII.GetBytes(_options.InitVector);
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = vector;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        using var transform = encrypt ? aes.CreateEncryptor() : aes.CreateDecryptor();
        var input = encrypt ? Encoding.UTF8.GetBytes(value) : Convert.FromBase64String(value);
        return transform.TransformFinalBlock(input, 0, input.Length);
    }

#pragma warning disable SYSLIB0060
    private static byte[] DeriveKey(string passphrase)
    {
        using var derived = new PasswordDeriveBytes(passphrase, null);
        return derived.GetBytes(32);
    }
#pragma warning restore SYSLIB0060
}
