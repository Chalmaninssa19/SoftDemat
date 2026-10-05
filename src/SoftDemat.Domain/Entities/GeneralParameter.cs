namespace SoftDemat.Domain.Entities;

public sealed class GeneralParameter : BaseEntity
{
    public const int SingletonId = 1;

    public string Cc { get; set; } = string.Empty;
    public string ArchiveFolder { get; set; } = string.Empty;
}
