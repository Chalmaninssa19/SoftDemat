using System.Security.Cryptography;
using System.Text;
using SoftDemat.Application.DTOs;
using SoftDemat.Application.Services.Interfaces;
using SoftDemat.Domain.Entities;
using SoftDemat.Domain.Exceptions;
using SoftDemat.Domain.Interfaces;

namespace SoftDemat.Infrastructure.Services;

public sealed class PasswordResetService : IPasswordResetService
{
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(1);
    private const string GenericRequestMessage =
        "Si un compte correspond à cette adresse e-mail, un lien de réinitialisation a été envoyé.";
    private readonly AuthResetStores _stores;
    private readonly AuthCrypto _crypto;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordResetNotifier _notifier;

    public PasswordResetService(
        AuthResetStores stores,
        AuthCrypto crypto,
        IUnitOfWork unitOfWork,
        IPasswordResetNotifier notifier)
    {
        _stores = stores;
        _crypto = crypto;
        _unitOfWork = unitOfWork;
        _notifier = notifier;
    }

    public async Task<string> RequestAsync(PasswordResetRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _stores.Users.GetUniqueByEmailAsync(normalizedEmail, cancellationToken);
        if (user?.Email is null)
            return GenericRequestMessage;

        var now = DateTime.UtcNow;
        var token = CreateToken();
        await _stores.ResetTokens.PurgeExpiredAsync(now, cancellationToken);
        await _stores.ResetTokens.RevokePendingAsync(user.Id, now, cancellationToken);
        await _stores.ResetTokens.AddAsync(new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = HashToken(token),
            ExpiresAt = now.Add(TokenLifetime),
            CreatedAt = now
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _notifier.SendResetLinkAsync(user.Email, token, cancellationToken);
        return GenericRequestMessage;
    }

    public async Task ResetAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var userId = await _stores.ResetTokens.ConsumeAsync(HashToken(request.Token), DateTime.UtcNow, cancellationToken)
            ?? throw new DomainException("Le lien de réinitialisation est invalide ou expiré.");
        var user = await _stores.Users.GetByIdAsync(userId, cancellationToken)
            ?? throw new DomainException("Le lien de réinitialisation est invalide ou expiré.");

        user.Password = _crypto.Hasher.Hash(request.NewPassword);
        await SetMustChangeAsync(user.Id, cancellationToken);
        await _stores.Sessions.RevokeActiveAsync(user.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task SetMustChangeAsync(int userId, CancellationToken cancellationToken)
    {
        var security = await _stores.Securities.GetAsync(userId, cancellationToken);
        if (security is null)
        {
            await _stores.Securities.AddAsync(
                new UserSecurity { UserId = userId, MustChangePassword = false },
                cancellationToken);
            return;
        }

        security.MustChangePassword = false;
    }

    private static string CreateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static string HashToken(string token)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
