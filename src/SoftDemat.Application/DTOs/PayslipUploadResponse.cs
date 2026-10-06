namespace SoftDemat.Application.DTOs;

public sealed record PayslipUploadRejection(string Name, string Reason);

public sealed record PayslipUploadResponse(
    string RelativeFolder,
    string FolderName,
    IReadOnlyList<string> Accepted,
    IReadOnlyList<PayslipUploadRejection> Rejected);
