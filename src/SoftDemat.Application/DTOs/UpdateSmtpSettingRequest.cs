namespace SoftDemat.Application.DTOs;

public sealed record UpdateSmtpSettingRequest(
    string Name,
    string Host,
    int Port,
    bool UseSsl,
    string? User,
    string? Password,
    string FromAddress);
