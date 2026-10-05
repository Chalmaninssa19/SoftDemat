namespace SoftDemat.Domain.Interfaces;

public sealed record IssuedTokens(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    string RefreshTokenHash,
    DateTime RefreshTokenExpiresAt);

public interface ITokenIssuer
{
    IssuedTokens Issue(int userId, string username, string name, string role, bool mustChangePassword);
    string HashRefreshToken(string refreshToken);
}
