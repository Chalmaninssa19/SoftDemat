using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftDemat.Api.Security;
using SoftDemat.Api.Uploads;
using SoftDemat.Application.DTOs;
using SoftDemat.Application.Services.Interfaces;
using SoftDemat.Domain.Rules;

namespace SoftDemat.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/payslip-files")]
[Produces("application/json")]
public sealed class PayslipFilesController : ControllerBase
{
    private readonly IPayslipFileService _service;
    private readonly IPayslipUploadService _uploads;

    public PayslipFilesController(IPayslipFileService service, IPayslipUploadService uploads)
    {
        _service = service;
        _uploads = uploads;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<PayslipFileResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] PayslipFileQuery query, CancellationToken cancellationToken)
        => Ok(ApiResponse<PaginatedResult<PayslipFileResponse>>.Ok(
            await _service.ListAsync(query, User.GetUserId(), cancellationToken)));

    [HttpGet("folders")]
    [ProducesResponseType(typeof(ApiResponse<PayslipFolderResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Folders([FromQuery] string? relativeFolder, CancellationToken cancellationToken)
        => Ok(ApiResponse<PayslipFolderResponse>.Ok(
            await _service.ListFoldersAsync(relativeFolder, User.GetUserId(), cancellationToken)));

    [HttpPost("uploads")]
    [RequestSizeLimit(PayslipUploadPolicy.MaxTransportBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = PayslipUploadPolicy.MaxTransportBytes, ValueCountLimit = 420)]
    [ProducesResponseType(typeof(ApiResponse<PayslipUploadResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Upload(
        [FromForm] List<IFormFile>? files,
        [FromForm] List<string>? paths,
        CancellationToken cancellationToken)
    {
        var created = await _uploads.ImportAsync(User.GetUserId(), PayslipUploadBinder.Bind(files, paths), cancellationToken);
        return Ok(ApiResponse<PayslipUploadResponse>.Ok(created, "Bulletins téléversés."));
    }
}
