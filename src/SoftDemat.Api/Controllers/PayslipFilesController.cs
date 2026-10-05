using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftDemat.Application.DTOs;
using SoftDemat.Application.Services.Interfaces;

namespace SoftDemat.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/payslip-files")]
[Produces("application/json")]
public sealed class PayslipFilesController : ControllerBase
{
    private readonly IPayslipFileService _service;

    public PayslipFilesController(IPayslipFileService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<PayslipFileResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] PayslipFileQuery query, CancellationToken cancellationToken)
        => Ok(ApiResponse<PaginatedResult<PayslipFileResponse>>.Ok(await _service.ListAsync(query, cancellationToken)));

    [HttpGet("folders")]
    [ProducesResponseType(typeof(ApiResponse<PayslipFolderResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Folders([FromQuery] string? relativeFolder, CancellationToken cancellationToken)
        => Ok(ApiResponse<PayslipFolderResponse>.Ok(await _service.ListFoldersAsync(relativeFolder, cancellationToken)));
}
