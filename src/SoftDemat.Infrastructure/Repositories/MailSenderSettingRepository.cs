using Microsoft.EntityFrameworkCore;
using SoftDemat.Domain.Entities;
using SoftDemat.Domain.Interfaces;
using SoftDemat.Infrastructure.Context;

namespace SoftDemat.Infrastructure.Repositories;

public sealed class MailSenderSettingRepository : IMailSenderSettingRepository
{
    private readonly SdtDbContext _context;

    public MailSenderSettingRepository(SdtDbContext context)
    {
        _context = context;
    }

    public Task<MailSenderSetting?> GetAsync(CancellationToken cancellationToken = default)
        => _context.MailSenderSettings.FirstOrDefaultAsync(setting => setting.Id == MailSenderSetting.SingletonId, cancellationToken);

    public Task AddAsync(MailSenderSetting setting, CancellationToken cancellationToken = default)
    {
        _context.MailSenderSettings.Add(setting);
        return Task.CompletedTask;
    }
}
