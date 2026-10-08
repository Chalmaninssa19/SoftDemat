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

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<SmtpSettingResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
        => Ok(ApiResponse<SmtpSettingResponse>.Ok(await _service.GetAsync(cancellationToken)));

    [Authorize(Roles = nameof(UserRole.Administrateur))]
    [HttpPut]
    [ProducesResponseType(typeof(ApiResponse<SmtpSettingResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromBody] UpdateSmtpSettingRequest request, CancellationToken cancellationToken)
        => Ok(ApiResponse<SmtpSettingResponse>.Ok(await _service.UpdateAsync(request, cancellationToken)));
}
