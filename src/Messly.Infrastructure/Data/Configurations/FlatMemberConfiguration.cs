using Messly.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Messly.Infrastructure.Data.Configurations;

public class FlatMemberConfiguration : BaseEntityConfiguration<FlatMember>
{
    public override void Configure(EntityTypeBuilder<FlatMember> builder)
    {
        base.Configure(builder);

        builder.ToTable("FlatMembers");

        builder.HasOne(fm => fm.Flat)
            .WithMany(f => f.Members)
            .HasForeignKey(fm => fm.FlatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(fm => fm.User)
            .WithMany(u => u.FlatMemberships)
            .HasForeignKey(fm => fm.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(fm => fm.Role)
            .WithMany(r => r.FlatMembers)
            .HasForeignKey(fm => fm.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(fm => new { fm.FlatId, fm.UserId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(fm => fm.FlatId);
        builder.HasIndex(fm => fm.UserId);
        builder.HasIndex(fm => fm.RoleId);
    }
}
