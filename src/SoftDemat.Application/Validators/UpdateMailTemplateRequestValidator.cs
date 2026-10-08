using FluentValidation;
using SoftDemat.Application.DTOs;

namespace SoftDemat.Application.Validators;

public sealed class UpdateMailTemplateRequestValidator : AbstractValidator<UpdateMailTemplateRequest>
{
    public UpdateMailTemplateRequestValidator()
    {
        RuleFor(request => request.MailType).NotEmpty().WithMessage("Le type de mail est obligatoire.");
        RuleFor(request => request.MailObject).NotEmpty().WithMessage("L'objet du mail est obligatoire.");
        RuleFor(request => request.MailContent).NotEmpty().WithMessage("Le contenu du mail est obligatoire.");
        RuleFor(request => request.MailCode).NotEmpty().WithMessage("Le code du mail est obligatoire.");
    }
}
