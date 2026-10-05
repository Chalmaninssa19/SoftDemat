using FluentValidation;
using SoftDemat.Application.DTOs;

namespace SoftDemat.Application.Validators;

public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(request => request.CurrentPassword).NotEmpty().WithMessage("Veuillez remplir les champs.");
        RuleFor(request => request.NewPassword).NotEmpty().WithMessage("Le mot de passe est obligatoire.");
        RuleFor(request => request.Confirmation)
            .Equal(request => request.NewPassword)
            .WithMessage("Vérifiez la confirmation du mot de passe.");
    }
}
