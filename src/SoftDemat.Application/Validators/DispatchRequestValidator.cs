using FluentValidation;
using SoftDemat.Application.DTOs;

namespace SoftDemat.Application.Validators;

public sealed class DispatchRequestValidator : AbstractValidator<DispatchRequest>
{
    public DispatchRequestValidator()
    {
        RuleFor(request => request.RelativeFolder).NotNull();
        RuleFor(request => request.MailTemplateId).GreaterThan(0).WithMessage("Choisissez un type de mail.");
        RuleFor(request => request.FileNames).NotEmpty().WithMessage("Aucune ligne sélectionnée.");
    }
}
