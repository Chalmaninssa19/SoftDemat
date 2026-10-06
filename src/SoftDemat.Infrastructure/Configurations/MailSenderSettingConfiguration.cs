using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftDemat.Domain.Entities;

namespace SoftDemat.Infrastructure.Configurations;

public sealed class MailSenderSettingConfiguration : IEntityTypeConfiguration<MailSenderSetting>
{
    public void Configure(EntityTypeBuilder<MailSenderSetting> builder)
    {
        builder.ToTable("G_MAIL_SENDER");
        builder.HasKey(setting => setting.Id);
        builder.Property(setting => setting.SenderTool).HasMaxLength(20);
        builder.Property(setting => setting.SenderAddress).HasMaxLength(200);
    }
}
