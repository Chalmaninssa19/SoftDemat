namespace SoftDemat.Infrastructure.Options;

public sealed class PayslipStorageOptions
{
    public const string SectionName = "PayslipStorage";
    public string RootPath { get; set; } = string.Empty;
}
