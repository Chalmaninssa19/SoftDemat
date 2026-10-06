using SoftDemat.Application.DTOs;

namespace SoftDemat.Application.Services.Interfaces;

public interface IDispatchService
{
    Task<DispatchResultResponse> SendAsync(
        DispatchRequest request,
        int userId,
        bool onSenderMachine,
        CancellationToken cancellationToken = default);
    Task<PaginatedResult<DispatchHistoryResponse>> SearchAsync(DispatchHistoryQuery query, CancellationToken cancellationToken = default);
}
