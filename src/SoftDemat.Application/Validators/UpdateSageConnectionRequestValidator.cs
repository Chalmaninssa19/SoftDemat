using FluentValidation;
using SoftDemat.Application.DTOs;

namespace SoftDemat.Application.Validators;

public sealed class UpdateSageConnectionRequestValidator : AbstractValidator<UpdateSageConnectionRequest>
{
    public UpdateSageConnectionRequestValidator()
    {
        RuleFor(request => request.Server).NotEmpty().WithMessage("Le serveur Sage est obligatoire.");
        RuleFor(request => request.DatabaseName).NotEmpty().WithMessage("La base Sage est obligatoire.");
        RuleFor(request => request.Login)
            .NotEmpty()
            .When(request => !request.WindowsAuthentication)
            .WithMessage("L'identifiant SQL est obligatoire.");
    }
}
