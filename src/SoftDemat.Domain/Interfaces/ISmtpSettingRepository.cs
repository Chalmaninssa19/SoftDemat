using SoftDemat.Domain.Entities;

namespace SoftDemat.Domain.Interfaces;

public interface ISmtpSettingRepository
{
    Task<IReadOnlyList<SmtpSetting>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SmtpSetting>> GetAllForUpdateAsync(CancellationToken cancellationToken = default);
    Task<SmtpSetting?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<SmtpSetting?> GetActiveAsync(CancellationToken cancellationToken = default);
    Task AddAsync(SmtpSetting setting, CancellationToken cancellationToken = default);
    void Remove(SmtpSetting setting);
}
