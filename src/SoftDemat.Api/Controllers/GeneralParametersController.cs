using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    public GeneralParametersController(IGeneralParameterService service)
    {
        _service = service;
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
}
