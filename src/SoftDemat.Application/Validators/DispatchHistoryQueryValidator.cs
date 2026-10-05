using FluentValidation;
using SoftDemat.Application.DTOs;

namespace SoftDemat.Application.Validators;

public sealed class DispatchHistoryQueryValidator : AbstractValidator<DispatchHistoryQuery>
{
    public DispatchHistoryQueryValidator()
    {
        RuleFor(query => query.From).NotEmpty().WithMessage("La période est obligatoire.");
        RuleFor(query => query.To).NotEmpty().WithMessage("La période est obligatoire.");
    }
}
