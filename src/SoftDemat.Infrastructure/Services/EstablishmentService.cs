using SoftDemat.Application.Common;
using SoftDemat.Application.DTOs;
using SoftDemat.Application.Services.Interfaces;
using SoftDemat.Domain.Interfaces;

namespace SoftDemat.Infrastructure.Services;

public sealed class EstablishmentService : IEstablishmentService
{
    private readonly IEstablishmentRepository _establishments;

    public EstablishmentService(IEstablishmentRepository establishments)
    {
        _establishments = establishments;
    }

    public async Task<PaginatedResult<EstablishmentResponse>> SearchAsync(
        PaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        var all = await _establishments.GetAllAsync(cancellationToken);
        IEnumerable<Domain.Entities.Establishment> filtered = all;
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            filtered = all.Where(establishment =>
                establishment.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
                || establishment.Code.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        var list = filtered.ToList();
        var page = PageRequest.Normalize(query.Page, query.Size);
        var items = list.Skip(page.Skip).Take(page.Size).Select(establishment => new EstablishmentResponse(establishment.Code, establishment.Name)).ToList();
        return new PaginatedResult<EstablishmentResponse>(items, list.Count, page.Page, page.Size, PageRequest.PageCount(list.Count, page.Size));
    }
}
