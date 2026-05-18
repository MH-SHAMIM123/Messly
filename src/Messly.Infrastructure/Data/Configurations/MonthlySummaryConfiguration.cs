using Messly.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Messly.Infrastructure.Data.Configurations;

public class MonthlySummaryConfiguration : BaseEntityConfiguration<MonthlySummary>
{
    public override void Configure(EntityTypeBuilder<MonthlySummary> builder)
    {
        base.Configure(builder);

        builder.ToTable("MonthlySummaries");

        builder.Property(m => m.TotalExpenses).HasPrecision(18, 2);
        builder.Property(m => m.TotalDeposits).HasPrecision(18, 2);
        builder.Property(m => m.MealRate).HasPrecision(18, 2);
        builder.Property(m => m.NetBalance).HasPrecision(18, 2);

        builder.HasOne(m => m.Flat)
            .WithMany(f => f.MonthlySummaries)
            .HasForeignKey(m => m.FlatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => new { m.FlatId, m.Year, m.Month })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}
