using Microsoft.Extensions.Logging;
using SoftDemat.Application.DTOs;
using SoftDemat.Application.Services.Interfaces;
using SoftDemat.Domain.Entities;
using SoftDemat.Domain.Exceptions;
using SoftDemat.Domain.Interfaces;

namespace SoftDemat.Infrastructure.Services;

public sealed class GeneralParameterService : IGeneralParameterService
{
    private readonly IGeneralParameterRepository _parameters;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GeneralParameterService> _logger;

    public GeneralParameterService(
        IGeneralParameterRepository parameters,
        IUnitOfWork unitOfWork,
        ILogger<GeneralParameterService> logger)
    {
        _parameters = parameters;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<GeneralParameterResponse> GetAsync(CancellationToken cancellationToken = default)
    {
        var parameter = await GetRequiredAsync(cancellationToken);
        return ToResponse(parameter);
    }

    public async Task<GeneralParameterResponse> UpdateAsync(
        UpdateGeneralParameterRequest request,
        CancellationToken cancellationToken = default)
    {
        var parameter = await GetRequiredAsync(cancellationToken);
        parameter.ArchiveFolder = request.ArchiveFolder.Trim();
        parameter.Cc = request.Cc?.Trim() ?? string.Empty;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Paramètres d'envoi mis à jour");
        return ToResponse(parameter);
    }

    private async Task<GeneralParameter> GetRequiredAsync(CancellationToken cancellationToken)
        => await _parameters.GetAsync(cancellationToken)
            ?? throw new NotFoundException("Paramètres généraux introuvables.");

    private static GeneralParameterResponse ToResponse(GeneralParameter parameter)
        => new(parameter.ArchiveFolder, parameter.Cc);
}
