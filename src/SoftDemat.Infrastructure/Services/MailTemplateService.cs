using SoftDemat.Application.DTOs;
using SoftDemat.Application.Services.Interfaces;
using SoftDemat.Domain.Entities;
using SoftDemat.Domain.Exceptions;
using SoftDemat.Domain.Interfaces;

namespace SoftDemat.Infrastructure.Services;

public sealed class MailTemplateService : IMailTemplateService
{
    private readonly IMailTemplateRepository _templates;
    private readonly IUnitOfWork _unitOfWork;

    public MailTemplateService(IMailTemplateRepository templates, IUnitOfWork unitOfWork)
    {
        _templates = templates;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<MailTemplateResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var templates = await _templates.GetAllAsync(cancellationToken);
        return templates.Select(ToResponse).ToList();
    }

    public async Task<MailTemplateResponse> CreateAsync(UpdateMailTemplateRequest request, CancellationToken cancellationToken = default)
    {
        var template = new MailTemplate();
        Apply(template, request);
        await _templates.AddAsync(template, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToResponse(template);
    }

    public async Task<MailTemplateResponse> UpdateAsync(
        int id,
        UpdateMailTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var template = await _templates.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Modèle de mail introuvable.");
        Apply(template, request);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToResponse(template);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var template = await _templates.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Modèle de mail introuvable.");
        _templates.Remove(template);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static void Apply(MailTemplate template, UpdateMailTemplateRequest request)
    {
        template.MailType = request.MailType.Trim();
        template.MailObject = request.MailObject.Trim();
        template.MailContent = request.MailContent;
        template.MailCode = request.MailCode.Trim();
    }

    private static MailTemplateResponse ToResponse(MailTemplate template)
        => new(template.Id, template.MailType, template.MailObject, template.MailContent, template.MailCode);
}
