using SoftDemat.Domain.Entities;

namespace SoftDemat.Domain.Interfaces;

public interface IGeneralParameterRepository
{
    Task<GeneralParameter?> GetAsync(CancellationToken cancellationToken = default);
}
