namespace SoftDemat.Application.DTOs;

public sealed record LoginResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    bool MustChangePassword,
    SessionResponse Session);
