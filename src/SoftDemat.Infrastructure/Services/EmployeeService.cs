using SoftDemat.Application.Common;
using SoftDemat.Application.DTOs;
using SoftDemat.Application.Services.Interfaces;
using SoftDemat.Domain.Entities;
using SoftDemat.Domain.Interfaces;

namespace SoftDemat.Infrastructure.Services;

public sealed class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employees;

    public EmployeeService(IEmployeeRepository employees)
    {
        _employees = employees;
    }

    public async Task<PaginatedResult<EmployeeResponse>> SearchAsync(EmployeeQuery query, CancellationToken cancellationToken = default)
    {
        var page = PageRequest.Normalize(query.Page, query.Size);
        var result = await _employees.SearchAsync(query.Search, query.EstablishmentCode, page.Skip, page.Size, cancellationToken);
        var items = result.Items.Select(ToResponse).ToList();
        return new PaginatedResult<EmployeeResponse>(
            items,
            result.TotalCount,
            page.Page,
            page.Size,
            PageRequest.PageCount(result.TotalCount, page.Size));
    }

    private static EmployeeResponse ToResponse(CurrentEmployee employee)
        => new(
            employee.Matricule,
            employee.LastName,
            employee.FirstName,
            employee.FullName,
            employee.Email,
            employee.EstablishmentCode,
            employee.EstablishmentName);
}
