using Microsoft.EntityFrameworkCore;
using SoftDemat.Domain.Entities;
using SoftDemat.Domain.Interfaces;
using SoftDemat.Infrastructure.Context;

namespace SoftDemat.Infrastructure.Repositories;

public sealed class SmtpSettingRepository : ISmtpSettingRepository
{
    private readonly SdtDbContext _context;

    public SmtpSettingRepository(SdtDbContext context)
    {
        _context = context;
    }

    public Task<SmtpSetting?> GetAsync(CancellationToken cancellationToken = default)
        => _context.SmtpSettings.FirstOrDefaultAsync(setting => setting.Id == SmtpSetting.SingletonId, cancellationToken);

    public Task AddAsync(SmtpSetting setting, CancellationToken cancellationToken = default)
    {
        _context.SmtpSettings.Add(setting);
        return Task.CompletedTask;
    }
}
