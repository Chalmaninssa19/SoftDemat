namespace SoftDemat.Application.DTOs;

public sealed record ResetPasswordRequest(string Token, string NewPassword, string Confirmation);
