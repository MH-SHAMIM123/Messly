using Messly.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Messly.Infrastructure.Data.Configurations;

public class ExpenseConfiguration : BaseEntityConfiguration<Expense>
{
    public override void Configure(EntityTypeBuilder<Expense> builder)
    {
        base.Configure(builder);

        builder.ToTable("Expenses");

        builder.Property(e => e.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.Amount)
            .HasPrecision(18, 2);

        builder.Property(e => e.ExpenseType)
            .HasConversion<int>();

        builder.HasOne(e => e.Flat)
            .WithMany(f => f.Expenses)
            .HasForeignKey(e => e.FlatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.PaidBy)
            .WithMany(u => u.PaidExpenses)
            .HasForeignKey(e => e.PaidByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Category)
            .WithMany(c => c.Expenses)
            .HasForeignKey(e => e.ExpenseCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.FlatId, e.ExpenseDate });
        builder.HasIndex(e => e.PaidByUserId);
        builder.HasIndex(e => e.ExpenseCategoryId);
    }
}
