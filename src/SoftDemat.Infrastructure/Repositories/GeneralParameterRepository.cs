using Microsoft.EntityFrameworkCore;
using SoftDemat.Domain.Entities;
using SoftDemat.Domain.Interfaces;
using SoftDemat.Infrastructure.Context;

namespace SoftDemat.Infrastructure.Repositories;

public sealed class GeneralParameterRepository : IGeneralParameterRepository
{
    private readonly SdtDbContext _context;

    public GeneralParameterRepository(SdtDbContext context)
    {
        _context = context;
    }

    public Task<GeneralParameter?> GetAsync(CancellationToken cancellationToken = default)
        => _context.GeneralParameters.FirstOrDefaultAsync(
            parameter => parameter.Id == GeneralParameter.SingletonId,
            cancellationToken);
}
