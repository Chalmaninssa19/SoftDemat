using Microsoft.EntityFrameworkCore;
using SoftDemat.Domain.Entities;
using SoftDemat.Domain.Interfaces;
using SoftDemat.Infrastructure.Context;

namespace SoftDemat.Infrastructure.Repositories;

public sealed class EmployeeRepository : IEmployeeRepository
{
    private readonly SageDbContext _context;

    public EmployeeRepository(SageDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CurrentEmployee>> GetCurrentAsync(CancellationToken cancellationToken = default)
        => await _context.Employees.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<(IReadOnlyList<CurrentEmployee> Items, int TotalCount)> SearchAsync(
        string? search,
        string? establishmentCode,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Employees.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(establishmentCode))
        {
            var code = establishmentCode.Trim();
            query = query.Where(employee => employee.EstablishmentCode == code);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(employee =>
                employee.Matricule.Contains(term)
                || employee.LastName.Contains(term)
                || employee.FirstName.Contains(term));
        }

        var ordered = query.OrderBy(employee => employee.Matricule);
        var total = await ordered.CountAsync(cancellationToken);
        var items = await ordered.Skip(skip).Take(take).ToListAsync(cancellationToken);
        return (items, total);
    }
}
