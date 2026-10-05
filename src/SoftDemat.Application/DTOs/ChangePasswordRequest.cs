namespace SoftDemat.Application.DTOs;

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword, string Confirmation);
