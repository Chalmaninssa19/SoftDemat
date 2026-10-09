using FluentValidation;
using SoftDemat.Application.DTOs;

namespace SoftDemat.Application.Validators;

public sealed class UpdateSmtpSettingRequestValidator : AbstractValidator<UpdateSmtpSettingRequest>
{
    public UpdateSmtpSettingRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(100).WithMessage("Le nom du serveur SMTP est obligatoire.");
        RuleFor(request => request.Host).NotEmpty().MaximumLength(200).WithMessage("Le serveur SMTP est obligatoire.");
        RuleFor(request => request.Port).InclusiveBetween(1, 65535).WithMessage("Le port SMTP est invalide.");
        RuleFor(request => request.User).MaximumLength(200);
        RuleFor(request => request.FromAddress).NotEmpty().EmailAddress().WithMessage("L'expéditeur SMTP est invalide.");
    }
}
