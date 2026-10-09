namespace SoftDemat.Application.DTOs;

public sealed record SmtpSettingResponse(
    int Id,
    string Name,
    bool IsActive,
    string Host,
    int Port,
    bool UseSsl,
    string User,
    bool HasPassword,
    string FromAddress);
