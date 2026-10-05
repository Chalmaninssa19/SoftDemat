using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftDemat.Application.DTOs;
using SoftDemat.Application.Services.Interfaces;
using SoftDemat.Domain.Enums;

namespace SoftDemat.Api.Controllers;

[ApiController]
[Authorize(Roles = nameof(UserRole.Administrateur))]
[Route("api/sage-connections")]
[Produces("application/json")]
public sealed class SageConnectionsController : ControllerBase
{
    private readonly ISageConnectionService _service;

    public SageConnectionsController(ISageConnectionService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<SageConnectionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
        => Ok(ApiResponse<SageConnectionResponse>.Ok(await _service.GetAsync(cancellationToken)));

    [HttpPut]
    [ProducesResponseType(typeof(ApiResponse<SageConnectionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromBody] UpdateSageConnectionRequest request, CancellationToken cancellationToken)
        => Ok(ApiResponse<SageConnectionResponse>.Ok(await _service.UpdateAsync(request, cancellationToken)));

    [HttpPost("tests")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Test([FromBody] UpdateSageConnectionRequest request, CancellationToken cancellationToken)
    {
        await _service.TestAsync(request, cancellationToken);
        return Ok(ApiResponse<string>.Ok("Connexion Sage réussie."));
    }
}
