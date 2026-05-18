using Messly.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Messly.Infrastructure.Data.Configurations;

public class DepositConfiguration : BaseEntityConfiguration<Deposit>
{
    public override void Configure(EntityTypeBuilder<Deposit> builder)
    {
        base.Configure(builder);

        builder.ToTable("Deposits");

        builder.Property(d => d.Amount)
            .HasPrecision(18, 2);

        builder.Property(d => d.Notes)
            .HasMaxLength(500);

        builder.Property(d => d.ReferenceNumber)
            .HasMaxLength(100);

        builder.HasOne(d => d.Flat)
            .WithMany(f => f.Deposits)
            .HasForeignKey(d => d.FlatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.User)
            .WithMany(u => u.Deposits)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(d => new { d.FlatId, d.DepositDate });
        builder.HasIndex(d => d.UserId);
    }
}
