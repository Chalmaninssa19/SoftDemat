using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftDemat.Api.Security;
using SoftDemat.Application.DTOs;
using SoftDemat.Application.Services.Interfaces;
using SoftDemat.Domain.Enums;

namespace SoftDemat.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/general-parameters")]
[Produces("application/json")]
public sealed class GeneralParametersController : ControllerBase
{
    private readonly IGeneralParameterService _service;
    private readonly IWorkstationToolService _tools;

    public GeneralParametersController(IGeneralParameterService service, IWorkstationToolService tools)
    {
        _service = service;
        _tools = tools;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GeneralParameterResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
        => Ok(ApiResponse<GeneralParameterResponse>.Ok(await _service.GetAsync(cancellationToken)));

    [Authorize(Roles = nameof(UserRole.Administrateur))]
    [HttpPut]
    [ProducesResponseType(typeof(ApiResponse<GeneralParameterResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromBody] UpdateGeneralParameterRequest request, CancellationToken cancellationToken)
        => Ok(ApiResponse<GeneralParameterResponse>.Ok(await _service.UpdateAsync(request, cancellationToken)));

    [Authorize(Roles = nameof(UserRole.Administrateur))]
    [HttpPost("archive-browse")]
    [ProducesResponseType(typeof(ApiResponse<ArchiveFolderPickResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Browse(CancellationToken cancellationToken)
        => Ok(ApiResponse<ArchiveFolderPickResponse>.Ok(
            await _tools.PickArchiveFolderAsync(LocalRequest.IsLoopback(HttpContext), cancellationToken)));

    [Authorize(Roles = nameof(UserRole.Administrateur))]
    [HttpGet("outlook-sender")]
    [ProducesResponseType(typeof(ApiResponse<OutlookSenderResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Outlook(CancellationToken cancellationToken)
        => Ok(ApiResponse<OutlookSenderResponse>.Ok(
            await _tools.ReadOutlookSenderAsync(LocalRequest.IsLoopback(HttpContext), cancellationToken)));
}
