using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SoftDemat.Api.Security;
using SoftDemat.Application.DTOs;
using SoftDemat.Application.Services.Interfaces;

namespace SoftDemat.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/auth")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [EnableRateLimiting("login")]
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        => Ok(ApiResponse<LoginResponse>.Ok(await _authService.LoginAsync(request, cancellationToken)));

    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
        => Ok(ApiResponse<LoginResponse>.Ok(await _authService.RefreshAsync(request, cancellationToken)));

    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        await _authService.LogoutAsync(request, cancellationToken);
        return Ok(ApiResponse<string>.Ok("Déconnecté."));
    }

    [HttpPut("password")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        await _authService.ChangePasswordAsync(User.GetUserId(), request, cancellationToken);
        return Ok(ApiResponse<string>.Ok("Mot de passe modifié."));
    }

    [HttpGet("session")]
    [ProducesResponseType(typeof(ApiResponse<SessionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Session(CancellationToken cancellationToken)
        => Ok(ApiResponse<SessionResponse>.Ok(await _authService.GetSessionAsync(User.GetUserId(), cancellationToken)));
}
