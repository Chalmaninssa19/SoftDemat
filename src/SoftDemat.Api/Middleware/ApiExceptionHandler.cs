using Microsoft.AspNetCore.Diagnostics;
using SoftDemat.Application.DTOs;
using SoftDemat.Domain.Exceptions;

namespace SoftDemat.Api.Middleware;

public sealed class ApiExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ApiExceptionHandler> _logger;

    public ApiExceptionHandler(ILogger<ApiExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, message) = exception switch
        {
            UnauthorizedException unauthorized => (StatusCodes.Status401Unauthorized, unauthorized.Message),
            ForbiddenException forbidden => (StatusCodes.Status403Forbidden, forbidden.Message),
            NotFoundException notFound => (StatusCodes.Status404NotFound, notFound.Message),
            ConflictException conflict => (StatusCodes.Status409Conflict, conflict.Message),
            DomainException domain => (StatusCodes.Status400BadRequest, domain.Message),
            _ => (StatusCodes.Status500InternalServerError, "Une erreur interne s'est produite.")
        };

        if (status == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Erreur non gérée");

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(ApiResponse<object>.Fail(message), cancellationToken);
        return true;
    }
}
