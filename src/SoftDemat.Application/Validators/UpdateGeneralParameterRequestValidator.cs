using FluentValidation;
using SoftDemat.Application.DTOs;
using SoftDemat.Domain.Rules;

namespace SoftDemat.Application.Validators;

public sealed class UpdateGeneralParameterRequestValidator : AbstractValidator<UpdateGeneralParameterRequest>
{
    public UpdateGeneralParameterRequestValidator()
    {
        RuleFor(request => request.ArchiveFolder).NotEmpty().WithMessage("Le dossier d'archivage est obligatoire.");
        RuleFor(request => request.Cc)
            .EmailAddress()
            .When(request => !string.IsNullOrWhiteSpace(request.Cc))
            .WithMessage("L'adresse en copie cachée est invalide.");
        RuleFor(request => request.SenderTool)
            .Must(MailSenderTools.IsKnown)
            .WithMessage("Choisissez Outlook ou une adresse d'expéditeur.");
        RuleFor(request => request.SenderAddress)
            .NotEmpty()
            .EmailAddress()
            .When(request => request.SenderTool == MailSenderTools.Address)
            .WithMessage("L'adresse de l'expéditeur est invalide.");
    }
}
