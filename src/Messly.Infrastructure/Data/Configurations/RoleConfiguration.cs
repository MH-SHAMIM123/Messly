using Messly.Domain.Entities;
using Messly.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Messly.Infrastructure.Data.Configurations;

public class RoleConfiguration : BaseEntityConfiguration<Role>
{
    public override void Configure(EntityTypeBuilder<Role> builder)
    {
        base.Configure(builder);

        builder.ToTable("Roles");

        builder.Property(r => r.Name)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.RoleType)
            .HasConversion<int>()
            .IsRequired();

        builder.HasIndex(r => r.RoleType)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasData(
            new Role
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111101"),
                Name = "Manager",
                RoleType = RoleType.Manager,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Role
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111102"),
                Name = "Member",
                RoleType = RoleType.Member,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
    }
}
