namespace SoftDemat.Application.DTOs;

public sealed record SmtpSettingResponse(
    string Host,
    int Port,
    bool UseSsl,
    string User,
    bool HasPassword,
    string FromAddress);
