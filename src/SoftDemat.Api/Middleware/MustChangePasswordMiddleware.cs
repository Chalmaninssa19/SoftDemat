using SoftDemat.Application.DTOs;
using SoftDemat.Domain.Constants;

namespace SoftDemat.Api.Middleware;

public sealed class MustChangePasswordMiddleware
{
    private readonly RequestDelegate _next;

    public MustChangePasswordMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var mustChange = context.User.FindFirst(AuthConstants.MustChangePasswordClaim)?.Value == "true";
        if (mustChange && !IsAllowed(context.Request.Path))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(
                ApiResponse<object>.Fail("Vous devez changer votre mot de passe."));
            return;
        }

        await _next(context);
    }

    private static bool IsAllowed(PathString path)
        => path.StartsWithSegments("/api/auth/password")
            || path.StartsWithSegments("/api/auth/password-resets")
            || path.StartsWithSegments("/api/auth/logout")
            || path.StartsWithSegments("/api/auth/session")
            || path.StartsWithSegments("/api/auth/refresh");
}
