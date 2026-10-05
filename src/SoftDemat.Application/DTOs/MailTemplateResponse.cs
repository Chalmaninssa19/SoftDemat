namespace SoftDemat.Application.DTOs;

public sealed record MailTemplateResponse(int Id, string MailType, string MailObject, string MailContent, string MailCode);
