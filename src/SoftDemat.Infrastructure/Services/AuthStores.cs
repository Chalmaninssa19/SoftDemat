using SoftDemat.Domain.Interfaces;

namespace SoftDemat.Infrastructure.Services;

public sealed class AuthStores
{
    public AuthStores(
        IUserRepository users,
        IAuthSessionRepository sessions,
        IUserSecurityRepository securities,
        ISageConnectionRepository sageConnections)
    {
        Users = users;
        Sessions = sessions;
        Securities = securities;
        SageConnections = sageConnections;
    }

    public IUserRepository Users { get; }
    public IAuthSessionRepository Sessions { get; }
    public IUserSecurityRepository Securities { get; }
    public ISageConnectionRepository SageConnections { get; }
}
