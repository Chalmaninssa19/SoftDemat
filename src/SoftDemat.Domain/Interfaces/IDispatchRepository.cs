using SoftDemat.Domain.Entities;

namespace SoftDemat.Domain.Interfaces;

public interface IDispatchRepository
{
    Task<IReadOnlyList<PayslipDispatch>> GetByPayPeriodAsync(
        IReadOnlyCollection<string> employeeNumbers,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<PayslipDispatch> Items, int TotalCount)> SearchAsync(
        DateTime from,
        DateTime to,
        bool sent,
        IReadOnlyCollection<string>? employeeNumbers,
        string? employeeNumber,
        string? search,
        bool descending,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task AddAsync(PayslipDispatch dispatch, CancellationToken cancellationToken = default);
}
