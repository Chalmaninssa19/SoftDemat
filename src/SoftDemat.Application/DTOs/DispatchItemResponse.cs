namespace SoftDemat.Application.DTOs;

public sealed record DispatchItemResponse(string Matricule, string FileName, bool Sent, string Detail);
