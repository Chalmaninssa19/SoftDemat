namespace SoftDemat.Application.DTOs;

public sealed record UpdateMailTemplateRequest(string MailType, string MailObject, string MailContent, string MailCode);
