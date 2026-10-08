namespace SoftDemat.Application.DTOs;

public sealed record CreateUserRequest(
    string Name,
    string Username,
    string Pc,
    int RoleId,
    string Password,
    string PasswordConfirmation,
    string? Email = null);
