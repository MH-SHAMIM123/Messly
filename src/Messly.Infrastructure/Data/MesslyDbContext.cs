using Messly.Domain.Common;
using Messly.Domain.Entities;
using Messly.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Messly.Infrastructure.Data;

public class MesslyDbContext(DbContextOptions<MesslyDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<User> AppUsers => Set<User>();
    public DbSet<Role> AppRoles => Set<Role>();
    public DbSet<Flat> Flats => Set<Flat>();
    public DbSet<FlatMember> FlatMembers => Set<FlatMember>();
    public DbSet<Meal> Meals => Set<Meal>();
    public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Deposit> Deposits => Set<Deposit>();
    public DbSet<MonthlySummary> MonthlySummaries => Set<MonthlySummary>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MesslyDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
