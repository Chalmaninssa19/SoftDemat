using SoftDemat.Domain.Entities;

namespace SoftDemat.Domain.Interfaces;

public interface IMailTemplateRepository
{
    Task<IReadOnlyList<MailTemplate>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<MailTemplate?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(MailTemplate template, CancellationToken cancellationToken = default);
    void Remove(MailTemplate template);
}
