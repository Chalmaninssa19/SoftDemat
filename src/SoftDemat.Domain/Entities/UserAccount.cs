using SoftDemat.Domain.Enums;

namespace SoftDemat.Domain.Entities;

public sealed class UserAccount : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Utilisateur;
    public string Pc { get; set; } = string.Empty;
}
