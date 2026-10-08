using Microsoft.EntityFrameworkCore;
using SoftDemat.Domain.Constants;
using SoftDemat.Domain.Entities;
using SoftDemat.Domain.Interfaces;
using SoftDemat.Infrastructure.Context;

namespace SoftDemat.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly SdtDbContext _context;

    public UserRepository(SdtDbContext context)
    {
        _context = context;
    }

    public Task<UserAccount?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => _context.Users.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

    public Task<UserAccount?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        => _context.Users.FirstOrDefaultAsync(user => user.Username == username, cancellationToken);

    public async Task<UserAccount?> GetUniqueByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        var matches = await _context.Users
            .AsNoTracking()
            .Where(user => user.Email == normalized)
            .Take(2)
            .ToListAsync(cancellationToken);
        return matches.Count == 1 ? matches[0] : null;
    }

    public Task<bool> UsernameExistsAsync(string username, int? exceptId, CancellationToken cancellationToken = default)
        => _context.Users.AnyAsync(
            user => user.Username == username && (exceptId == null || user.Id != exceptId),
            cancellationToken);

    public Task<bool> EmailExistsAsync(string email, int? exceptId, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return _context.Users.AnyAsync(
            user => user.Email == normalized && (exceptId == null || user.Id != exceptId),
            cancellationToken);
    }

    public async Task<(IReadOnlyList<UserAccount> Items, int TotalCount)> SearchAsync(
        string? search,
        string? sortBy,
        bool descending,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Users.AsNoTracking().Where(user => user.Id != AuthConstants.SystemUserId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(user => user.Name.Contains(term)
                || user.Username.Contains(term)
                || user.Pc.Contains(term)
                || (user.Email != null && user.Email.Contains(term)));
        }

        query = sortBy?.ToLowerInvariant() switch
        {
            "username" => descending ? query.OrderByDescending(user => user.Username) : query.OrderBy(user => user.Username),
            "role" => descending ? query.OrderByDescending(user => user.Role) : query.OrderBy(user => user.Role),
            "pc" => descending ? query.OrderByDescending(user => user.Pc) : query.OrderBy(user => user.Pc),
            _ => descending ? query.OrderByDescending(user => user.Name) : query.OrderBy(user => user.Name)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip(skip).Take(take).ToListAsync(cancellationToken);
        return (items, total);
    }

    public async Task AddAsync(UserAccount user, CancellationToken cancellationToken = default)
        => await _context.Users.AddAsync(user, cancellationToken);

    public void Remove(UserAccount user) => _context.Users.Remove(user);
}
