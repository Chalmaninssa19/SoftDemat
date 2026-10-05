namespace SoftDemat.Application.DTOs;

public sealed record PayslipFileResponse(
    string FileName,
    string Matricule,
    string LastName,
    string FirstName,
    string FullName,
    string Email,
    string EstablishmentCode,
    string EstablishmentName,
    DateTime PayDate,
    bool AlreadySent,
    bool SelectedByDefault);
