using FluentValidation;
using SoftDemat.Application.DTOs;

namespace SoftDemat.Application.Validators;

public sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().WithMessage("Le nom est obligatoire.");
        RuleFor(request => request.Username).NotEmpty().WithMessage("L'identifiant est obligatoire.");
        RuleFor(request => request.Pc).NotEmpty().WithMessage("Le poste est obligatoire.");
        RuleFor(request => request.RoleId).InclusiveBetween(0, 1).WithMessage("Rôle inconnu.");
        RuleFor(request => request.Password).NotEmpty().WithMessage("Le mot de passe est obligatoire.");
        RuleFor(request => request.PasswordConfirmation)
            .Equal(request => request.Password)
            .WithMessage("Vérifiez votre mot de passe.");
    }
}
