using SoftDemat.Domain.Entities;

namespace SoftDemat.Domain.Interfaces;

public interface IAuthSessionRepository
{
    Task AddAsync(AuthSession session, CancellationToken cancellationToken = default);
    Task<AuthSession?> GetByHashAsync(string refreshTokenHash, CancellationToken cancellationToken = default);
    Task RevokeActiveAsync(int userId, CancellationToken cancellationToken = default);
}
