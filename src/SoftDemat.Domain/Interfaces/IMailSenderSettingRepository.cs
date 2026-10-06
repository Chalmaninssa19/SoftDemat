using SoftDemat.Domain.Entities;

namespace SoftDemat.Domain.Interfaces;

public interface IMailSenderSettingRepository
{
    Task<MailSenderSetting?> GetAsync(CancellationToken cancellationToken = default);
    Task AddAsync(MailSenderSetting setting, CancellationToken cancellationToken = default);
}
