namespace SoftDemat.Domain.Entities;

public sealed class AuthSession
{
    public Guid Id { get; set; }
    public int UserId { get; set; }
    public string RefreshTokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
