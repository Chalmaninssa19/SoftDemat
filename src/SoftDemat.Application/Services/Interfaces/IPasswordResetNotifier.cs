namespace SoftDemat.Application.Services.Interfaces;

public interface IPasswordResetNotifier
{
    Task SendResetLinkAsync(string email, string token, CancellationToken cancellationToken = default);
}
