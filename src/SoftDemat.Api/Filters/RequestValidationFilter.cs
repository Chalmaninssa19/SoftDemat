using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SoftDemat.Application.DTOs;

namespace SoftDemat.Api.Filters;

public sealed class RequestValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _services;

    public RequestValidationFilter(IServiceProvider services)
    {
        _services = services;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null || argument is CancellationToken)
                continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (_services.GetService(validatorType) is not IValidator validator)
                continue;

            var result = await validator.ValidateAsync(new ValidationContext<object>(argument), context.HttpContext.RequestAborted);
            if (result.IsValid)
                continue;

            context.Result = new BadRequestObjectResult(ApiResponse<object>.Fail(Join(result)));
            return;
        }

        await next();
    }

    private static string Join(ValidationResult result)
        => string.Join(" ", result.Errors.Select(error => error.ErrorMessage).Distinct());
}
