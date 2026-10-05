namespace SoftDemat.Application.DTOs;

public sealed record PayslipFolderResponse(string RelativeFolder, IReadOnlyList<string> Children);
