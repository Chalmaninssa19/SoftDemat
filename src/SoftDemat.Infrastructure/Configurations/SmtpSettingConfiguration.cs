using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftDemat.Domain.Entities;

namespace SoftDemat.Infrastructure.Configurations;

public sealed class SmtpSettingConfiguration : IEntityTypeConfiguration<SmtpSetting>
{
    public void Configure(EntityTypeBuilder<SmtpSetting> builder)
    {
        builder.ToTable("G_SMTP");
        builder.HasKey(setting => setting.Id);
        builder.Property(setting => setting.Host).HasMaxLength(200);
        builder.Property(setting => setting.UserName).HasMaxLength(200);
        builder.Property(setting => setting.Password).HasMaxLength(500);
        builder.Property(setting => setting.FromAddress).HasMaxLength(200);
    }
}
