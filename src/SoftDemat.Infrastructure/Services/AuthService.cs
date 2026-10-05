using Microsoft.Extensions.Logging;
using SoftDemat.Application.DTOs;
using SoftDemat.Application.Services.Interfaces;
using SoftDemat.Domain.Constants;
using SoftDemat.Domain.Entities;
using SoftDemat.Domain.Exceptions;
using SoftDemat.Domain.Interfaces;

namespace SoftDemat.Infrastructure.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IAuthSessionRepository _sessions;
    private readonly IUserSecurityRepository _securities;
    private readonly ISageConnectionRepository _sageConnections;
    private readonly IPasswordHasher _hasher;
    private readonly ILegacyPasswordProtector _legacy;
    private readonly ITokenIssuer _tokens;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuthService> _logger;

    public AuthService(AuthStores stores, AuthCrypto crypto, IUnitOfWork unitOfWork, ILogger<AuthService> logger)
    {
        _users = stores.Users;
        _sessions = stores.Sessions;
        _securities = stores.Securities;
        _sageConnections = stores.SageConnections;
        _hasher = crypto.Hasher;
        _legacy = crypto.Legacy;
        _tokens = crypto.Tokens;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByUsernameAsync(request.Username.Trim(), cancellationToken);
        if (user is null || !PasswordMatches(user, request.Password))
            throw new UnauthorizedException(AuthConstants.LoginFailureMessage);

        if (!user.Password.StartsWith("$2", StringComparison.Ordinal))
            user.Password = _hasher.Hash(request.Password);

        var security = await _securities.GetAsync(user.Id, cancellationToken);
        return await IssueAsync(user, security?.MustChangePassword ?? false, cancellationToken);
    }

    public async Task<LoginResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var session = await FindActiveSessionAsync(request.RefreshToken, cancellationToken);
        session.RevokedAt = DateTime.UtcNow;
        var user = await _users.GetByIdAsync(session.UserId, cancellationToken)
            ?? throw new UnauthorizedException(AuthConstants.LoginFailureMessage);
        var security = await _securities.GetAsync(user.Id, cancellationToken);
        return await IssueAsync(user, security?.MustChangePassword ?? false, cancellationToken);
    }

    public async Task LogoutAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var hash = _tokens.HashRefreshToken(request.RefreshToken);
        var session = await _sessions.GetByHashAsync(hash, cancellationToken);
        if (session is null || session.RevokedAt is not null)
            return;

        session.RevokedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(userId, cancellationToken)
            ?? throw new UnauthorizedException(AuthConstants.LoginFailureMessage);
        if (!PasswordMatches(user, request.CurrentPassword))
            throw new DomainException("Vérifiez le mot de passe.");

        user.Password = _hasher.Hash(request.NewPassword);
        await SetMustChangeAsync(user.Id, false, cancellationToken);
        await _sessions.RevokeActiveAsync(user.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Mot de passe modifié pour {Username}", user.Username);
    }

    public async Task<SessionResponse> GetSessionAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Utilisateur introuvable.");
        return await ToSessionAsync(user, cancellationToken);
    }

    private async Task<LoginResponse> IssueAsync(UserAccount user, bool mustChangePassword, CancellationToken cancellationToken)
    {
        var issued = _tokens.Issue(user.Id, user.Username, user.Name, user.Role.ToString(), mustChangePassword);
        await _sessions.AddAsync(new AuthSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RefreshTokenHash = issued.RefreshTokenHash,
            ExpiresAt = issued.RefreshTokenExpiresAt,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var session = await ToSessionAsync(user, cancellationToken);
        return new LoginResponse(issued.AccessToken, issued.AccessTokenExpiresAt, issued.RefreshToken, mustChangePassword, session);
    }

    private async Task<AuthSession> FindActiveSessionAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var session = await _sessions.GetByHashAsync(_tokens.HashRefreshToken(refreshToken), cancellationToken);
        if (session is null || session.RevokedAt is not null || session.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedException(AuthConstants.LoginFailureMessage);

        return session;
    }

    private bool PasswordMatches(UserAccount user, string password)
    {
        if (_hasher.Verify(password, user.Password))
            return true;
        if (!_legacy.CanUse || user.Password.StartsWith("$2", StringComparison.Ordinal))
            return false;

        try
        {
            return _legacy.Encrypt(password) == user.Password;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Mot de passe historique illisible pour {Username}", user.Username);
            return false;
        }
    }

    private async Task SetMustChangeAsync(int userId, bool mustChange, CancellationToken cancellationToken)
    {
        var security = await _securities.GetAsync(userId, cancellationToken);
        if (security is null)
        {
            await _securities.AddAsync(new UserSecurity { UserId = userId, MustChangePassword = mustChange }, cancellationToken);
            return;
        }

        security.MustChangePassword = mustChange;
    }

    private async Task<SessionResponse> ToSessionAsync(UserAccount user, CancellationToken cancellationToken)
    {
        var sage = await _sageConnections.GetAsync(cancellationToken);
        return new SessionResponse(user.Id, user.Name, user.Username, user.Role.ToString(), sage?.DatabaseName);
    }
}
