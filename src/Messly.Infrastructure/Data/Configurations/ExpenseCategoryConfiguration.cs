using Messly.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Messly.Infrastructure.Data.Configurations;

public class ExpenseCategoryConfiguration : BaseEntityConfiguration<ExpenseCategory>
{
    public override void Configure(EntityTypeBuilder<ExpenseCategory> builder)
    {
        base.Configure(builder);

        builder.ToTable("ExpenseCategories");

        builder.Property(c => c.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.HasOne(c => c.Flat)
            .WithMany(f => f.ExpenseCategories)
            .HasForeignKey(c => c.FlatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => new { c.FlatId, c.Name })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}
