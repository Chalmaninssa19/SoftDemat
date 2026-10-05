using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftDemat.Api.Security;
using SoftDemat.Application.DTOs;
using SoftDemat.Application.Services.Interfaces;
using SoftDemat.Domain.Enums;

namespace SoftDemat.Api.Controllers;

[ApiController]
[Authorize(Roles = nameof(UserRole.Administrateur))]
[Route("api/users")]
[Produces("application/json")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<UserResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] PaginationQuery query, CancellationToken cancellationToken)
        => Ok(ApiResponse<PaginatedResult<UserResponse>>.Ok(await _userService.SearchAsync(query, cancellationToken)));

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var created = await _userService.CreateAsync(request, cancellationToken);
        return Created($"/api/users/{created.Id}", ApiResponse<UserResponse>.Ok(created, "Utilisateur créé."));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
        => Ok(ApiResponse<UserResponse>.Ok(await _userService.UpdateAsync(id, request, cancellationToken)));

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _userService.DeleteAsync(id, User.GetUserId(), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/password-resets")]
    [ProducesResponseType(typeof(ApiResponse<ResetPasswordResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetPassword(int id, CancellationToken cancellationToken)
        => Ok(ApiResponse<ResetPasswordResponse>.Ok(await _userService.ResetPasswordAsync(id, cancellationToken)));
}
