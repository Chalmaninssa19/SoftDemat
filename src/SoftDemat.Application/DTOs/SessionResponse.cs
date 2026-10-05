namespace SoftDemat.Application.DTOs;

public sealed record SessionResponse(int Id, string Name, string Username, string Role, string? SageDatabaseName);
