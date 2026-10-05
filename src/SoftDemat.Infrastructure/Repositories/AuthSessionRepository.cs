using Microsoft.EntityFrameworkCore;
using SoftDemat.Domain.Entities;
using SoftDemat.Domain.Interfaces;
using SoftDemat.Infrastructure.Context;

namespace SoftDemat.Infrastructure.Repositories;

public sealed class AuthSessionRepository : IAuthSessionRepository
{
    private readonly SdtDbContext _context;

    public AuthSessionRepository(SdtDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AuthSession session, CancellationToken cancellationToken = default)
        => await _context.AuthSessions.AddAsync(session, cancellationToken);

    public Task<AuthSession?> GetByHashAsync(string refreshTokenHash, CancellationToken cancellationToken = default)
        => _context.AuthSessions.FirstOrDefaultAsync(session => session.RefreshTokenHash == refreshTokenHash, cancellationToken);

    public async Task RevokeActiveAsync(int userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var sessions = await _context.AuthSessions
            .Where(session => session.UserId == userId && session.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var session in sessions)
            session.RevokedAt = now;
    }
}
