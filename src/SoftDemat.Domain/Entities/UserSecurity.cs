namespace SoftDemat.Domain.Entities;

public sealed class UserSecurity
{
    public int UserId { get; set; }
    public bool MustChangePassword { get; set; }
}
