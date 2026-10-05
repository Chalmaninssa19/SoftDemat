namespace SoftDemat.Application.DTOs;

public sealed record UpdateUserRequest(
    string Name,
    string Username,
    string Pc,
    int RoleId,
    string? Password,
    string? PasswordConfirmation);
