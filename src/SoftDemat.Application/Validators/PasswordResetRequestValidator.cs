using FluentValidation;
using SoftDemat.Application.DTOs;

namespace SoftDemat.Application.Validators;

public sealed class PasswordResetRequestValidator : AbstractValidator<PasswordResetRequest>
{
    public PasswordResetRequestValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty().WithMessage("L'adresse e-mail est obligatoire.")
            .EmailAddress().WithMessage("L'adresse e-mail n'est pas valide.")
            .MaximumLength(254).WithMessage("L'adresse e-mail est trop longue.");
    }
}
