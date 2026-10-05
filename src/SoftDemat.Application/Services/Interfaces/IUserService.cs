using SoftDemat.Application.DTOs;

namespace SoftDemat.Application.Services.Interfaces;

public interface IUserService
{
    Task<PaginatedResult<UserResponse>> SearchAsync(PaginationQuery query, CancellationToken cancellationToken = default);
    Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserResponse> UpdateAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, int currentUserId, CancellationToken cancellationToken = default);
    Task<ResetPasswordResponse> ResetPasswordAsync(int id, CancellationToken cancellationToken = default);
}
