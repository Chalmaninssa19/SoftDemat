using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftDemat.Application.DTOs;
using SoftDemat.Application.Services.Interfaces;
using SoftDemat.Domain.Enums;

namespace SoftDemat.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/smtp-settings")]
[Produces("application/json")]
public sealed class SmtpSettingsController : ControllerBase
{
    private readonly ISmtpSettingService _service;

    public SmtpSettingsController(ISmtpSettingService service)
    {
        _service = service;
    }

    [Authorize(Roles = nameof(UserRole.Administrateur))]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SmtpSettingResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
        => Ok(ApiResponse<IReadOnlyList<SmtpSettingResponse>>.Ok(await _service.GetAllAsync(cancellationToken)));

    [Authorize(Roles = nameof(UserRole.Administrateur))]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SmtpSettingResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] UpdateSmtpSettingRequest request, CancellationToken cancellationToken)
    {
        var created = await _service.CreateAsync(request, cancellationToken);
        return Created($"/api/smtp-settings/{created.Id}", ApiResponse<SmtpSettingResponse>.Ok(created));
    }

    [Authorize(Roles = nameof(UserRole.Administrateur))]
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SmtpSettingResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateSmtpSettingRequest request,
        CancellationToken cancellationToken)
        => Ok(ApiResponse<SmtpSettingResponse>.Ok(await _service.UpdateAsync(id, request, cancellationToken)));

    [Authorize(Roles = nameof(UserRole.Administrateur))]
    [HttpPut("{id:int}/activation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Activate(int id, CancellationToken cancellationToken)
    {
        await _service.ActivateAsync(id, cancellationToken);
        return NoContent();
    }

    [Authorize(Roles = nameof(UserRole.Administrateur))]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
