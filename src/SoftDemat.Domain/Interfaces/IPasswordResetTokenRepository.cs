using SoftDemat.Domain.Entities;

namespace SoftDemat.Domain.Interfaces;

public interface IPasswordResetTokenRepository
{
    Task RevokePendingAsync(int userId, DateTime revokedAt, CancellationToken cancellationToken = default);
    Task PurgeExpiredAsync(DateTime now, CancellationToken cancellationToken = default);
    Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default);
    Task<int?> ConsumeAsync(string tokenHash, DateTime consumedAt, CancellationToken cancellationToken = default);
}
