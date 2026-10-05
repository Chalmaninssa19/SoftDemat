using SoftDemat.Domain.Entities;

namespace SoftDemat.Domain.Interfaces;

public interface IEmployeeRepository
{
    Task<IReadOnlyList<CurrentEmployee>> GetCurrentAsync(CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<CurrentEmployee> Items, int TotalCount)> SearchAsync(
        string? search,
        string? establishmentCode,
        int skip,
        int take,
        CancellationToken cancellationToken = default);
}
