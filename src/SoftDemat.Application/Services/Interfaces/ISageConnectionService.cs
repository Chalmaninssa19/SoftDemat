using SoftDemat.Application.DTOs;

namespace SoftDemat.Application.Services.Interfaces;

public interface ISageConnectionService
{
    Task<SageConnectionResponse> GetAsync(CancellationToken cancellationToken = default);
    Task<SageConnectionResponse> UpdateAsync(UpdateSageConnectionRequest request, CancellationToken cancellationToken = default);
    Task TestAsync(UpdateSageConnectionRequest request, CancellationToken cancellationToken = default);
}
