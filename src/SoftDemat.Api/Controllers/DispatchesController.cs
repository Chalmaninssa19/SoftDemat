using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftDemat.Application.DTOs;
using SoftDemat.Application.Services.Interfaces;

namespace SoftDemat.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/dispatches")]
[Produces("application/json")]
public sealed class DispatchesController : ControllerBase
{
    private readonly IDispatchService _service;

    public DispatchesController(IDispatchService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<DispatchHistoryResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] DispatchHistoryQuery query, CancellationToken cancellationToken)
        => Ok(ApiResponse<PaginatedResult<DispatchHistoryResponse>>.Ok(await _service.SearchAsync(query, cancellationToken)));

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<DispatchResultResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] DispatchRequest request, CancellationToken cancellationToken)
        => Ok(ApiResponse<DispatchResultResponse>.Ok(await _service.SendAsync(request, cancellationToken)));
}
