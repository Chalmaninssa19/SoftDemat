namespace SoftDemat.Application.DTOs;

public sealed class DispatchHistoryQuery
{
    public DateTime From { get; init; }
    public DateTime To { get; init; }
    public string? EstablishmentCode { get; init; }
    public string? EmployeeMatricule { get; init; }
    public bool Sent { get; init; }
    public int Page { get; init; } = 1;
    public int Size { get; init; } = 20;
    public string? Search { get; init; }
    public string? SortBy { get; init; }
    public string SortDirection { get; init; } = "desc";
}
