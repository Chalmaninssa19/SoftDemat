using SoftDemat.Domain.Entities;

namespace SoftDemat.Domain.Interfaces;

public interface ISmtpSettingRepository
{
    Task<SmtpSetting?> GetAsync(CancellationToken cancellationToken = default);
    Task AddAsync(SmtpSetting setting, CancellationToken cancellationToken = default);
}
