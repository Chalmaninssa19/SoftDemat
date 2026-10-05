using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftDemat.Domain.Entities;

namespace SoftDemat.Infrastructure.Configurations;

public sealed class SageConnectionSettingsConfiguration : IEntityTypeConfiguration<SageConnectionSettings>
{
    public void Configure(EntityTypeBuilder<SageConnectionSettings> builder)
    {
        builder.ToTable("T_BDD_SAGE");
        builder.HasKey(settings => settings.Id);
        builder.Property(settings => settings.Id).HasColumnName("ID");
        builder.Property(settings => settings.Server).HasColumnName("SERVEUR");
        builder.Property(settings => settings.Login).HasColumnName("TLOGIN");
        builder.Property(settings => settings.Password).HasColumnName("TMDP");
        builder.Property(settings => settings.AuthenticationType).HasColumnName("TYPE_AUTH");
        builder.Property(settings => settings.DatabaseName).HasColumnName("NOM_BD");
    }
}
