using SoftDemat.Application.DTOs;

namespace SoftDemat.Application.Services.Interfaces;

public interface IMailTemplateService
{
    Task<IReadOnlyList<MailTemplateResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<MailTemplateResponse> UpdateAsync(int id, UpdateMailTemplateRequest request, CancellationToken cancellationToken = default);
}
