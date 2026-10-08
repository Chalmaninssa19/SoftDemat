using Microsoft.EntityFrameworkCore;
using SoftDemat.Domain.Entities;
using SoftDemat.Domain.Interfaces;
using SoftDemat.Infrastructure.Context;

namespace SoftDemat.Infrastructure.Repositories;

public sealed class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly SdtDbContext _context;

    public PasswordResetTokenRepository(SdtDbContext context)
    {
        _context = context;
    }

    public async Task RevokePendingAsync(int userId, DateTime revokedAt, CancellationToken cancellationToken = default)
    {
        var tokens = await _context.PasswordResetTokens
            .Where(token => token.UserId == userId && token.UsedAt == null)
            .ToListAsync(cancellationToken);
        foreach (var token in tokens)
            token.UsedAt = revokedAt;
    }

    public async Task PurgeExpiredAsync(DateTime now, CancellationToken cancellationToken = default)
    {
        await _context.PasswordResetTokens
            .Where(token => token.ExpiresAt < now)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default)
        => _context.PasswordResetTokens.AddAsync(token, cancellationToken).AsTask();

    public async Task<int?> ConsumeAsync(
        string tokenHash,
        DateTime consumedAt,
        CancellationToken cancellationToken = default)
    {
        var token = await _context.PasswordResetTokens
            .AsNoTracking()
            .Where(candidate => candidate.TokenHash == tokenHash
                && candidate.UsedAt == null
                && candidate.ExpiresAt > consumedAt)
            .Select(candidate => new { candidate.Id, candidate.UserId })
            .FirstOrDefaultAsync(cancellationToken);
        if (token is null)
            return null;

        var updated = await _context.PasswordResetTokens
            .Where(candidate => candidate.Id == token.Id
                && candidate.UsedAt == null
                && candidate.ExpiresAt > consumedAt)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(candidate => candidate.UsedAt, consumedAt),
                cancellationToken);
        return updated == 1 ? token.UserId : null;
    }
}
