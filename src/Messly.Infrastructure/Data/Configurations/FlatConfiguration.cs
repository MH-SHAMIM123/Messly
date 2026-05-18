using Messly.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Messly.Infrastructure.Data.Configurations;

public class FlatConfiguration : BaseEntityConfiguration<Flat>
{
    public override void Configure(EntityTypeBuilder<Flat> builder)
    {
        base.Configure(builder);

        builder.ToTable("Flats");

        builder.Property(f => f.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(f => f.Address)
            .HasMaxLength(500);

        builder.Property(f => f.Description)
            .HasMaxLength(1000);

        builder.Property(f => f.DefaultMealRate)
            .HasPrecision(18, 2);

        builder.HasOne(f => f.Creator)
            .WithMany(u => u.CreatedFlats)
            .HasForeignKey(f => f.CreatorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(f => f.CreatorId);
        builder.HasIndex(f => f.Name);
    }
}
