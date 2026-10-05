using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftDemat.Domain.Entities;

namespace SoftDemat.Infrastructure.Configurations;

public sealed class GeneralParameterConfiguration : IEntityTypeConfiguration<GeneralParameter>
{
    public void Configure(EntityTypeBuilder<GeneralParameter> builder)
    {
        builder.ToTable("D_PGENERAL");
        builder.HasKey(parameter => parameter.Id);
        builder.Property(parameter => parameter.Cc).HasColumnName("Cc");
        builder.Property(parameter => parameter.ArchiveFolder).HasColumnName("ArchFolder");
    }
}
