using SoftDemat.Application.DTOs;

namespace SoftDemat.Application.Services.Interfaces;

public interface IPasswordResetService
{
    Task<string> RequestAsync(PasswordResetRequest request, CancellationToken cancellationToken = default);
    Task ResetAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
}
