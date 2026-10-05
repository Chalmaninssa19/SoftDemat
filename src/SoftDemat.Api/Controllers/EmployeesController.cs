using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftDemat.Application.DTOs;
using SoftDemat.Application.Services.Interfaces;

namespace SoftDemat.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/employees")]
[Produces("application/json")]
public sealed class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _service;

    public EmployeesController(IEmployeeService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<EmployeeResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] EmployeeQuery query, CancellationToken cancellationToken)
        => Ok(ApiResponse<PaginatedResult<EmployeeResponse>>.Ok(await _service.SearchAsync(query, cancellationToken)));
}
