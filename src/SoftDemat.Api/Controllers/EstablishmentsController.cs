using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftDemat.Application.DTOs;
using SoftDemat.Application.Services.Interfaces;

namespace SoftDemat.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/establishments")]
[Produces("application/json")]
public sealed class EstablishmentsController : ControllerBase
{
    private readonly IEstablishmentService _service;

    public EstablishmentsController(IEstablishmentService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<EstablishmentResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] PaginationQuery query, CancellationToken cancellationToken)
        => Ok(ApiResponse<PaginatedResult<EstablishmentResponse>>.Ok(await _service.SearchAsync(query, cancellationToken)));
}
