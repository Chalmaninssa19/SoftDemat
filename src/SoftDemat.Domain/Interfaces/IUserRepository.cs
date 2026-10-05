using SoftDemat.Domain.Entities;

namespace SoftDemat.Domain.Interfaces;

public interface IUserRepository
{
    Task<UserAccount?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<UserAccount?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> UsernameExistsAsync(string username, int? exceptId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<UserAccount> Items, int TotalCount)> SearchAsync(
        string? search,
        string? sortBy,
        bool descending,
        int skip,
        int take,
        CancellationToken cancellationToken = default);
    Task AddAsync(UserAccount user, CancellationToken cancellationToken = default);
    void Remove(UserAccount user);
}
