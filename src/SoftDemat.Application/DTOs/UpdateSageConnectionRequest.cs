namespace SoftDemat.Application.DTOs;

public sealed record UpdateSageConnectionRequest(
    string Server,
    string DatabaseName,
    string? Login,
    string? Password,
    bool WindowsAuthentication);
