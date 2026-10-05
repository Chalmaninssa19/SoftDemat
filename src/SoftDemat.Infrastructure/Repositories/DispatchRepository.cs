using Microsoft.EntityFrameworkCore;
using SoftDemat.Domain.Entities;
using SoftDemat.Domain.Interfaces;
using SoftDemat.Infrastructure.Context;

namespace SoftDemat.Infrastructure.Repositories;

public sealed class DispatchRepository : IDispatchRepository
{
    private readonly SdtDbContext _context;

    public DispatchRepository(SdtDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PayslipDispatch>> GetByPayPeriodAsync(
        IReadOnlyCollection<string> employeeNumbers,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        if (employeeNumbers.Count == 0)
            return [];

        return await _context.Dispatches.AsNoTracking()
            .Where(dispatch => dispatch.PayDate >= from && dispatch.PayDate <= to)
            .Where(dispatch => employeeNumbers.Contains(dispatch.EmployeeNumber))
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<PayslipDispatch> Items, int TotalCount)> SearchAsync(
        DateTime from,
        DateTime to,
        bool sent,
        IReadOnlyCollection<string>? employeeNumbers,
        string? employeeNumber,
        string? search,
        bool descending,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        if (employeeNumbers is { Count: 0 })
            return ([], 0);

        var query = _context.Dispatches.AsNoTracking()
            .Where(dispatch => dispatch.SentAt >= from && dispatch.SentAt <= to && dispatch.Sent == sent);

        if (employeeNumbers is not null)
            query = query.Where(dispatch => employeeNumbers.Contains(dispatch.EmployeeNumber));

        if (!string.IsNullOrWhiteSpace(employeeNumber))
        {
            var matricule = employeeNumber.Trim();
            query = query.Where(dispatch => dispatch.EmployeeNumber.Trim() == matricule);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(dispatch => dispatch.Name.Contains(term) || dispatch.EmployeeNumber.Contains(term));
        }

        query = descending
            ? query.OrderByDescending(dispatch => dispatch.SentAt)
            : query.OrderBy(dispatch => dispatch.SentAt);

        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip(skip).Take(take).ToListAsync(cancellationToken);
        return (items, total);
    }

    public async Task AddAsync(PayslipDispatch dispatch, CancellationToken cancellationToken = default)
        => await _context.Dispatches.AddAsync(dispatch, cancellationToken);
}
