using SoftDemat.Domain.Interfaces;

namespace SoftDemat.Infrastructure.Services;

public sealed class AuthCrypto
{
    public AuthCrypto(IPasswordHasher hasher, ILegacyPasswordProtector legacy, ITokenIssuer tokens)
    {
        Hasher = hasher;
        Legacy = legacy;
        Tokens = tokens;
    }

    public IPasswordHasher Hasher { get; }
    public ILegacyPasswordProtector Legacy { get; }
    public ITokenIssuer Tokens { get; }
}
