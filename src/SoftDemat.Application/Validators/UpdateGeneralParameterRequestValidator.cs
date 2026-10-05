using FluentValidation;
using SoftDemat.Application.DTOs;

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
    }
}
