using SoftDemat.Application.DTOs;

namespace SoftDemat.Application.Services.Interfaces;

public interface IPayslipUploadService
{
    Task<PayslipUploadResponse> ImportAsync(
        int userId,
        IReadOnlyList<PayslipUploadItem> files,
        CancellationToken cancellationToken = default);
}
