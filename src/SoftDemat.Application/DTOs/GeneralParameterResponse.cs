namespace SoftDemat.Application.DTOs;

public sealed record GeneralParameterResponse(
    string ArchiveFolder,
    string Cc,
    string SenderTool,
    string SenderAddress,
    string SageFolder,
    string SenderEmail);
