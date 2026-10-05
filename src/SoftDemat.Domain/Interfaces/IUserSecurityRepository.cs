using SoftDemat.Domain.Entities;

namespace SoftDemat.Domain.Interfaces;

public interface IUserSecurityRepository
{
    Task<UserSecurity?> GetAsync(int userId, CancellationToken cancellationToken = default);
    Task AddAsync(UserSecurity security, CancellationToken cancellationToken = default);
}
