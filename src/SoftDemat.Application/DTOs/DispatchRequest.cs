namespace SoftDemat.Application.DTOs;

public sealed record DispatchRequest(string RelativeFolder, int MailTemplateId, IReadOnlyList<string> FileNames);
