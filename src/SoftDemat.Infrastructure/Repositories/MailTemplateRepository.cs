using Microsoft.EntityFrameworkCore;
using SoftDemat.Domain.Entities;
using SoftDemat.Domain.Interfaces;
using SoftDemat.Infrastructure.Context;

namespace SoftDemat.Infrastructure.Repositories;

public sealed class MailTemplateRepository : IMailTemplateRepository
{
    private readonly SdtDbContext _context;

    public MailTemplateRepository(SdtDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<MailTemplate>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.MailTemplates.AsNoTracking().OrderBy(template => template.MailType).ToListAsync(cancellationToken);

    public Task<MailTemplate?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => _context.MailTemplates.FirstOrDefaultAsync(template => template.Id == id, cancellationToken);

    public Task AddAsync(MailTemplate template, CancellationToken cancellationToken = default)
    {
        _context.MailTemplates.Add(template);
        return Task.CompletedTask;
    }

    public void Remove(MailTemplate template) => _context.MailTemplates.Remove(template);
}
