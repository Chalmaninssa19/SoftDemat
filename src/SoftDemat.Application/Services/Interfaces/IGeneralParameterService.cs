using SoftDemat.Application.DTOs;

namespace SoftDemat.Application.Services.Interfaces;

public interface IGeneralParameterService
{
    Task<GeneralParameterResponse> GetAsync(CancellationToken cancellationToken = default);
    Task<GeneralParameterResponse> UpdateAsync(UpdateGeneralParameterRequest request, CancellationToken cancellationToken = default);
}
