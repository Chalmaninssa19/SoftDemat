using FluentValidation;
using SoftDemat.Application.DTOs;
using SoftDemat.Domain.Rules;

namespace SoftDemat.Application.Validators;

public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(request => request.CurrentPassword).NotEmpty().WithMessage("Veuillez remplir les champs.");
        RuleFor(request => request.NewPassword)
            .Must(PasswordPolicy.IsSatisfied)
            .WithMessage(PasswordPolicy.FailureMessage);
        RuleFor(request => request.Confirmation)
            .Equal(request => request.NewPassword)
            .WithMessage("Vérifiez la confirmation du mot de passe.");
    }
}
