namespace SoftDemat.Domain.Entities;

public sealed class CurrentEmployee
{
    public string Matricule { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string EstablishmentCode { get; set; } = string.Empty;
    public string EstablishmentName { get; set; } = string.Empty;

    public string FullName => $"{LastName} {FirstName}".Trim();
}
