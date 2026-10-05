using FluentValidation;
using SoftDemat.Application.DTOs;

namespace SoftDemat.Application.Validators;

public sealed class UpdateMailTemplateRequestValidator : AbstractValidator<UpdateMailTemplateRequest>
{
    public UpdateMailTemplateRequestValidator()
    {
        RuleFor(request => request.MailObject).NotEmpty().WithMessage("L'objet du mail est obligatoire.");
        RuleFor(request => request.MailContent).NotEmpty().WithMessage("Le contenu du mail est obligatoire.");
    }
}
