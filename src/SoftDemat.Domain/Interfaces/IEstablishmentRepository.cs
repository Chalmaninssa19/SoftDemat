using SoftDemat.Domain.Entities;

namespace SoftDemat.Domain.Interfaces;

public interface IEstablishmentRepository
{
    Task<IReadOnlyList<Establishment>> GetAllAsync(CancellationToken cancellationToken = default);
}
