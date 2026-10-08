using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftDemat.Domain.Entities;

namespace SoftDemat.Infrastructure.Configurations;

public sealed class UserAccountConfiguration : IEntityTypeConfiguration<UserAccount>
{
    public void Configure(EntityTypeBuilder<UserAccount> builder)
    {
        builder.ToTable("G_USERS");
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Name).HasColumnName("Name");
        builder.Property(user => user.Username).HasColumnName("Username");
        builder.Property(user => user.Email).HasColumnName("Email").HasMaxLength(254).IsRequired(false);
        builder.Property(user => user.Password).HasColumnName("Password");
        builder.Property(user => user.Role).HasColumnName("IdRole");
        builder.Property(user => user.Pc).HasColumnName("PC");
    }
}
