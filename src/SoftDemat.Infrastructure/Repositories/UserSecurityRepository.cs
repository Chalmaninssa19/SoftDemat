using Microsoft.EntityFrameworkCore;
using SoftDemat.Domain.Entities;
using SoftDemat.Domain.Interfaces;
using SoftDemat.Infrastructure.Context;

namespace SoftDemat.Infrastructure.Repositories;

public sealed class UserSecurityRepository : IUserSecurityRepository
{
    private readonly SdtDbContext _context;

    public UserSecurityRepository(SdtDbContext context)
    {
        _context = context;
    }

    public Task<UserSecurity?> GetAsync(int userId, CancellationToken cancellationToken = default)
        => _context.UserSecurities.FirstOrDefaultAsync(security => security.UserId == userId, cancellationToken);

    public async Task AddAsync(UserSecurity security, CancellationToken cancellationToken = default)
        => await _context.UserSecurities.AddAsync(security, cancellationToken);
}
