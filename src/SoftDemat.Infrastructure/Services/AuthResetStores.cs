using SoftDemat.Domain.Interfaces;

namespace SoftDemat.Infrastructure.Services;

public sealed class AuthResetStores
{
    public AuthResetStores(
        IUserRepository users,
        IAuthSessionRepository sessions,
        IUserSecurityRepository securities,
        IPasswordResetTokenRepository resetTokens)
    {
        Users = users;
        Sessions = sessions;
        Securities = securities;
        ResetTokens = resetTokens;
    }

    public IUserRepository Users { get; }
    public IAuthSessionRepository Sessions { get; }
    public IUserSecurityRepository Securities { get; }
    public IPasswordResetTokenRepository ResetTokens { get; }
}
