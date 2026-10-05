namespace SoftDemat.Application.DTOs;

public sealed record EmployeeResponse(
    string Matricule,
    string LastName,
    string FirstName,
    string FullName,
    string Email,
    string EstablishmentCode,
    string EstablishmentName);
