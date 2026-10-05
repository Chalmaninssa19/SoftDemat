using SoftDemat.Domain.Entities;

namespace SoftDemat.Domain.Interfaces;

public interface ISageConnectionRepository
{
    Task<SageConnectionSettings?> GetAsync(CancellationToken cancellationToken = default);
    Task AddAsync(SageConnectionSettings settings, CancellationToken cancellationToken = default);
}
