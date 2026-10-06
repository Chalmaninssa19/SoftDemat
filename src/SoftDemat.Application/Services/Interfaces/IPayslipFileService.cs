using SoftDemat.Application.DTOs;

namespace SoftDemat.Application.Services.Interfaces;

public interface IPayslipFileService
{
    Task<PayslipFolderResponse> ListFoldersAsync(string? relativeFolder, int userId, CancellationToken cancellationToken = default);
    Task<PaginatedResult<PayslipFileResponse>> ListAsync(PayslipFileQuery query, int userId, CancellationToken cancellationToken = default);
}
