namespace SoftDemat.Domain.Entities;

public sealed class SageConnectionSettings : BaseEntity
{
    public string Server { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool AuthenticationType { get; set; } = true;
    public string DatabaseName { get; set; } = string.Empty;
}
