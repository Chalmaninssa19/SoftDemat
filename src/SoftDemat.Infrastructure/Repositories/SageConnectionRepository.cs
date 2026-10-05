using Microsoft.EntityFrameworkCore;
using SoftDemat.Domain.Entities;
using SoftDemat.Domain.Interfaces;
using SoftDemat.Infrastructure.Context;

namespace SoftDemat.Infrastructure.Repositories;

public sealed class SageConnectionRepository : ISageConnectionRepository
{
    private readonly SdtDbContext _context;

    public SageConnectionRepository(SdtDbContext context)
    {
        _context = context;
    }

    public Task<SageConnectionSettings?> GetAsync(CancellationToken cancellationToken = default)
        => _context.SageConnections.OrderBy(settings => settings.Id).FirstOrDefaultAsync(cancellationToken);

    public async Task AddAsync(SageConnectionSettings settings, CancellationToken cancellationToken = default)
        => await _context.SageConnections.AddAsync(settings, cancellationToken);
}
