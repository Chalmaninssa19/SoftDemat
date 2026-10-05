using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SoftDemat.Domain.Constants;
using SoftDemat.Domain.Exceptions;
using SoftDemat.Domain.Interfaces;
using SoftDemat.Infrastructure.Options;

namespace SoftDemat.Infrastructure.Security;

public sealed class JwtTokenIssuer : ITokenIssuer
{
    private readonly JwtOptions _options;

    public JwtTokenIssuer(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public IssuedTokens Issue(int userId, string username, string name, string role, bool mustChangePassword)
    {
        if (_options.SigningKey.Length < 32)
            throw new DomainException("La clé de signature JWT n'est pas configurée.");

        var expires = DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, username),
            new("display_name", name),
            new(ClaimTypes.Role, role),
            new(AuthConstants.MustChangePasswordClaim, mustChangePassword ? "true" : "false")
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var token = new JwtSecurityToken(
            _options.Issuer,
            _options.Audience,
            claims,
            expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        var refresh = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var refreshExpires = DateTime.UtcNow.AddDays(_options.RefreshTokenDays);
        return new IssuedTokens(
            new JwtSecurityTokenHandler().WriteToken(token),
            expires,
            refresh,
            HashRefreshToken(refresh),
            refreshExpires);
    }

    public string HashRefreshToken(string refreshToken)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToHexString(hash);
    }
}
