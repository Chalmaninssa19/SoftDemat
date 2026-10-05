using FluentValidation;
using SoftDemat.Application.DTOs;

namespace SoftDemat.Application.Validators;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(request => request.Username).NotEmpty().WithMessage("Veuillez remplir les champs.");
        RuleFor(request => request.Password).NotEmpty().WithMessage("Veuillez remplir les champs.");
    }
}
