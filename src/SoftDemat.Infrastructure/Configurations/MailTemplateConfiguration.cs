using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftDemat.Domain.Entities;

namespace SoftDemat.Infrastructure.Configurations;

public sealed class MailTemplateConfiguration : IEntityTypeConfiguration<MailTemplate>
{
    public void Configure(EntityTypeBuilder<MailTemplate> builder)
    {
        builder.ToTable("D_PMAIL");
        builder.HasKey(template => template.Id);
        builder.Property(template => template.MailType).HasColumnName("MailType");
        builder.Property(template => template.MailObject).HasColumnName("MailObject");
        builder.Property(template => template.MailContent).HasColumnName("MailContent");
        builder.Property(template => template.MailCode).HasColumnName("MailCode");
    }
}
