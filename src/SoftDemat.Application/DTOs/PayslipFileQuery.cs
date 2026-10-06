namespace SoftDemat.Application.DTOs;

public sealed class PayslipFileQuery
{
    public string? RelativeFolder { get; init; }
    public string? EstablishmentCode { get; init; }
    public string? EmployeeMatricule { get; init; }
    public string? Matricule { get; init; }
    public string? Name { get; init; }
    public int Page { get; init; } = 1;
    public int Size { get; init; } = 20;
    public string? Search { get; init; }
    public string? SortBy { get; init; }
    public string SortDirection { get; init; } = "asc";
}
