using SoftDemat.Domain.Interfaces;

namespace SoftDemat.Infrastructure.Security;

public sealed class BcryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash) || !passwordHash.StartsWith("$2", StringComparison.Ordinal))
            return false;

        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}
