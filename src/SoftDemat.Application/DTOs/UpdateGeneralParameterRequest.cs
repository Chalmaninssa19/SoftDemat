namespace SoftDemat.Application.DTOs;

public sealed record UpdateGeneralParameterRequest(
    string ArchiveFolder,
    string? Cc,
    string SenderTool,
    string? SenderAddress,
    string? SageFolder = null,
    string? SenderEmail = null);
