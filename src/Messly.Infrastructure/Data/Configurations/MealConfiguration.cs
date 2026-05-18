using Messly.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Messly.Infrastructure.Data.Configurations;

public class MealConfiguration : BaseEntityConfiguration<Meal>
{
    public override void Configure(EntityTypeBuilder<Meal> builder)
    {
        base.Configure(builder);

        builder.ToTable("Meals");

        builder.Property(m => m.Notes)
            .HasMaxLength(500);

        builder.HasOne(m => m.Flat)
            .WithMany(f => f.Meals)
            .HasForeignKey(m => m.FlatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.User)
            .WithMany(u => u.Meals)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => new { m.FlatId, m.UserId, m.MealDate })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(m => new { m.FlatId, m.MealDate });
    }
}
