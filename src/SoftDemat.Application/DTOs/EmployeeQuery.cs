namespace SoftDemat.Application.DTOs;

public sealed class EmployeeQuery
{
    public int Page { get; init; } = 1;
    public int Size { get; init; } = 20;
    public string? Search { get; init; }
    public string? EstablishmentCode { get; init; }
    public string? SortBy { get; init; }
    public string SortDirection { get; init; } = "asc";
}
