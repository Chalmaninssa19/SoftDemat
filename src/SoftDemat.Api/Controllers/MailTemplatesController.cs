using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftDemat.Application.DTOs;
using SoftDemat.Application.Services.Interfaces;
using SoftDemat.Domain.Enums;

namespace SoftDemat.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/mail-templates")]
[Produces("application/json")]
public sealed class MailTemplatesController : ControllerBase
{
    private readonly IMailTemplateService _service;

    public MailTemplatesController(IMailTemplateService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<MailTemplateResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
        => Ok(ApiResponse<IReadOnlyList<MailTemplateResponse>>.Ok(await _service.GetAllAsync(cancellationToken)));

    [Authorize(Roles = nameof(UserRole.Administrateur))]
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<MailTemplateResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMailTemplateRequest request, CancellationToken cancellationToken)
        => Ok(ApiResponse<MailTemplateResponse>.Ok(await _service.UpdateAsync(id, request, cancellationToken)));
}
