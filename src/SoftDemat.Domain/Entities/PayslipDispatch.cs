namespace SoftDemat.Domain.Entities;

public sealed class PayslipDispatch : BaseEntity
{
    public string EmployeeNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime PayDate { get; set; }
    public bool Sent { get; set; }
}
