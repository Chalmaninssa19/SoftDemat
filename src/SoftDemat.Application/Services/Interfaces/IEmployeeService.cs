using SoftDemat.Application.DTOs;

namespace SoftDemat.Application.Services.Interfaces;

public interface IEmployeeService
{
    Task<PaginatedResult<EmployeeResponse>> SearchAsync(EmployeeQuery query, CancellationToken cancellationToken = default);
}

public interface IEstablishmentService
{
    Task<PaginatedResult<EstablishmentResponse>> SearchAsync(PaginationQuery query, CancellationToken cancellationToken = default);
}
