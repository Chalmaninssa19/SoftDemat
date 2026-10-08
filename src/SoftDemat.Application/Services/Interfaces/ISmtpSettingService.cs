using SoftDemat.Application.DTOs;

namespace SoftDemat.Application.Services.Interfaces;

public interface ISmtpSettingService
{
    Task<SmtpSettingResponse> GetAsync(CancellationToken cancellationToken = default);
    Task<SmtpSettingResponse> UpdateAsync(UpdateSmtpSettingRequest request, CancellationToken cancellationToken = default);
}
