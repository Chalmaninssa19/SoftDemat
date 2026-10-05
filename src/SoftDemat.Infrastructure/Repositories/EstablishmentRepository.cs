using Microsoft.EntityFrameworkCore;
using SoftDemat.Domain.Entities;
using SoftDemat.Domain.Interfaces;
using SoftDemat.Infrastructure.Context;

namespace SoftDemat.Infrastructure.Repositories;

public sealed class EstablishmentRepository : IEstablishmentRepository
{
    private readonly SageDbContext _context;

    public EstablishmentRepository(SageDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Establishment>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Establishments.AsNoTracking().OrderBy(establishment => establishment.Name).ToListAsync(cancellationToken);
}
