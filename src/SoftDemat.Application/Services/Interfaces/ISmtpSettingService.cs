using SoftDemat.Application.DTOs;

namespace SoftDemat.Application.Services.Interfaces;

public interface ISmtpSettingService
{
    Task<IReadOnlyList<SmtpSettingResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SmtpSettingResponse> CreateAsync(UpdateSmtpSettingRequest request, CancellationToken cancellationToken = default);
    Task<SmtpSettingResponse> UpdateAsync(int id, UpdateSmtpSettingRequest request, CancellationToken cancellationToken = default);
    Task ActivateAsync(int id, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
