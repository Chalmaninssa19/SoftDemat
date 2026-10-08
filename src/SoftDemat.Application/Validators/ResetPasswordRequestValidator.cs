using FluentValidation;
using SoftDemat.Application.DTOs;

namespace SoftDemat.Application.Validators;

public sealed class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(request => request.Token).NotEmpty().WithMessage("Le lien de réinitialisation est invalide.");
        RuleFor(request => request.NewPassword)
            .NotEmpty().WithMessage("Le mot de passe est obligatoire.")
            .MinimumLength(12).WithMessage("Le mot de passe doit contenir au moins 12 caractères.");
        RuleFor(request => request.Confirmation)
            .Equal(request => request.NewPassword)
            .WithMessage("Vérifiez la confirmation du mot de passe.");
    }
}
