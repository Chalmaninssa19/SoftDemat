namespace SoftDemat.Domain.Interfaces;

public interface ILegacyPasswordProtector
{
    bool CanUse { get; }
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
