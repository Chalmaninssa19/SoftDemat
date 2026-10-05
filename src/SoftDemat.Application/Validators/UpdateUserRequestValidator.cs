using FluentValidation;
using SoftDemat.Application.DTOs;

namespace SoftDemat.Application.Validators;

public sealed class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().WithMessage("Le nom est obligatoire.");
        RuleFor(request => request.Username).NotEmpty().WithMessage("L'identifiant est obligatoire.");
        RuleFor(request => request.Pc).NotEmpty().WithMessage("Le poste est obligatoire.");
        RuleFor(request => request.RoleId).InclusiveBetween(0, 1).WithMessage("Rôle inconnu.");
        RuleFor(request => request.PasswordConfirmation)
            .Equal(request => request.Password)
            .When(request => !string.IsNullOrEmpty(request.Password))
            .WithMessage("Vérifiez votre mot de passe.");
    }
}
