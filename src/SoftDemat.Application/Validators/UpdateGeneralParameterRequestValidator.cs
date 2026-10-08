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
            .WithMessage("Choisissez Outlook ou MailKit.");
        RuleFor(request => request.SenderEmail)
            .EmailAddress()
            .When(request => !string.IsNullOrWhiteSpace(request.SenderEmail))
            .WithMessage("L'e-mail expéditeur est invalide.");
    }
}
