namespace SoftDemat.Infrastructure.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; set; } = "SoftDemat";
    public string Audience { get; set; } = "SoftDemat";
    public string SigningKey { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 20;
    public int RefreshTokenDays { get; set; } = 7;
}
