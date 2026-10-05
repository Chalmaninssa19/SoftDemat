namespace SoftDemat.Application.DTOs;

public sealed record DispatchHistoryResponse(
    int Id,
    string Matricule,
    string Name,
    string Email,
    DateTime SentAt,
    DateTime PayDate,
    string FileName,
    bool Sent);
