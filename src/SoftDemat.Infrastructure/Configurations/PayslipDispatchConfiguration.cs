using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftDemat.Domain.Entities;

namespace SoftDemat.Infrastructure.Configurations;

public sealed class PayslipDispatchConfiguration : IEntityTypeConfiguration<PayslipDispatch>
{
    public void Configure(EntityTypeBuilder<PayslipDispatch> builder)
    {
        builder.ToTable("D_DEMAT");
        builder.HasKey(dispatch => dispatch.Id);
        builder.Property(dispatch => dispatch.EmployeeNumber).HasColumnName("No");
        builder.Property(dispatch => dispatch.Name).HasColumnName("Name");
        builder.Property(dispatch => dispatch.SentAt).HasColumnName("DateSend");
        builder.Property(dispatch => dispatch.FilePath).HasColumnName("FilePath");
        builder.Property(dispatch => dispatch.FileName).HasColumnName("FileName");
        builder.Property(dispatch => dispatch.PayDate).HasColumnName("DatePaie");
        builder.Property(dispatch => dispatch.Sent).HasColumnName("Status");
    }
}
