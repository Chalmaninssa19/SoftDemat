using Microsoft.EntityFrameworkCore;
using SoftDemat.Domain.Entities;
using SoftDemat.Infrastructure.Configurations;

namespace SoftDemat.Infrastructure.Context;

public sealed class SdtDbContext : DbContext
{
    public SdtDbContext(DbContextOptions<SdtDbContext> options) : base(options)
    {
    }

    public DbSet<UserAccount> Users => Set<UserAccount>();
    public DbSet<PayslipDispatch> Dispatches => Set<PayslipDispatch>();
    public DbSet<MailTemplate> MailTemplates => Set<MailTemplate>();
    public DbSet<GeneralParameter> GeneralParameters => Set<GeneralParameter>();
    public DbSet<SageConnectionSettings> SageConnections => Set<SageConnectionSettings>();
    public DbSet<AuthSession> AuthSessions => Set<AuthSession>();
    public DbSet<UserSecurity> UserSecurities => Set<UserSecurity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserAccountConfiguration());
        modelBuilder.ApplyConfiguration(new PayslipDispatchConfiguration());
        modelBuilder.ApplyConfiguration(new MailTemplateConfiguration());
        modelBuilder.ApplyConfiguration(new GeneralParameterConfiguration());
        modelBuilder.ApplyConfiguration(new SageConnectionSettingsConfiguration());
        modelBuilder.ApplyConfiguration(new AuthSessionConfiguration());
        modelBuilder.ApplyConfiguration(new UserSecurityConfiguration());
    }
}
