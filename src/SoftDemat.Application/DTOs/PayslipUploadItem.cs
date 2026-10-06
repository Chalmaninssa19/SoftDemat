namespace SoftDemat.Application.DTOs;

public sealed record PayslipUploadItem(string BrowserPath, Stream Content, long Length);
