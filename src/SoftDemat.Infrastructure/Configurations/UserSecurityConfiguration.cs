using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftDemat.Domain.Entities;

namespace SoftDemat.Infrastructure.Configurations;

public sealed class UserSecurityConfiguration : IEntityTypeConfiguration<UserSecurity>
{
    public void Configure(EntityTypeBuilder<UserSecurity> builder)
    {
        builder.ToTable("G_USER_SECURITY");
        builder.HasKey(security => security.UserId);
    }
}
