namespace SoftDemat.Application.DTOs;

public sealed record SageConnectionResponse(string Server, string DatabaseName, string Login, bool HasPassword);
