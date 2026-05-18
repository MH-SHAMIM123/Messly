using Messly.Application.Interfaces.Security;
using Messly.Application.Services;
using Messly.Application.Services.Security;
using Messly.Domain.Entities;
using Messly.Domain.Enums;
using Messly.Infrastructure.Data;
using Messly.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Messly.Tests;

public class BillingCalculationServiceTests
{
    [Fact]
    public async Task CalculateMealRateAsync_DividesExpensesByMeals()
    {
        var (service, db, flatId, userId, _) = await CreateServiceAsync();
        await SeedExpenseAndMealAsync(db, flatId, userId, 3000, 3);

        var rate = await service.CalculateMealRateAsync(2026, 5);

        Assert.Equal(1000m, rate);
        await db.DisposeAsync();
    }

    [Fact]
    public async Task CalculateMealRateAsync_ReturnsZero_WhenNoMeals()
    {
        var (service, db, flatId, userId, categoryId) = await CreateServiceAsync();
        db.Expenses.Add(new Expense
        {
            FlatId = flatId,
            PaidByUserId = userId,
            ExpenseCategoryId = categoryId,
            Title = "Grocery",
            Amount = 1000,
            ExpenseDate = new DateOnly(2026, 5, 10),
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var rate = await service.CalculateMealRateAsync(2026, 5);

        Assert.Equal(0m, rate);
        await db.DisposeAsync();
    }

    [Fact]
    public async Task CalculateMealRateAsync_ReturnsZero_WhenNoExpenses()
    {
        var (service, db, flatId, userId, _) = await CreateServiceAsync();
        db.Meals.Add(new Meal
        {
            FlatId = flatId,
            UserId = userId,
            MealDate = new DateOnly(2026, 5, 10),
            BreakfastCount = 1,
            LunchCount = 1,
            DinnerCount = 1,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var rate = await service.CalculateMealRateAsync(2026, 5);

        Assert.Equal(0m, rate);
        await db.DisposeAsync();
    }

    [Fact]
    public async Task GetMemberBalancesAsync_ComputesDepositMinusMealCost()
    {
        var (service, db, flatId, userId, categoryId) = await CreateServiceAsync();
        await SeedExpenseAndMealAsync(db, flatId, userId, 3000, 3);
        db.Deposits.Add(new Deposit
        {
            FlatId = flatId,
            UserId = userId,
            Amount = 5000,
            DepositDate = new DateOnly(2026, 5, 15),
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var balances = await service.GetMemberBalancesAsync(2026, 5);
        var row = Assert.Single(balances);

        Assert.Equal(3, row.TotalMeals);
        Assert.Equal(3000m, row.MealCost);
        Assert.Equal(5000m, row.TotalDeposits);
        Assert.Equal(2000m, row.Balance);

        await db.DisposeAsync();
    }

    [Fact]
    public async Task GenerateMonthlySummaryAsync_PersistsTotals()
    {
        var (service, db, flatId, userId, _) = await CreateServiceAsync();
        await SeedExpenseAndMealAsync(db, flatId, userId, 3000, 3);

        await service.GenerateMonthlySummaryAsync(2026, 5);

        var stored = await db.MonthlySummaries
            .FirstOrDefaultAsync(s => s.FlatId == flatId && s.Year == 2026 && s.Month == 5);

        Assert.NotNull(stored);
        Assert.Equal(3000m, stored.TotalExpenses);
        Assert.Equal(3, stored.TotalMeals);
        Assert.Equal(1000m, stored.MealRate);

        await db.DisposeAsync();
    }

    [Fact]
    public async Task BuildFinancialSummaryAsync_IncludesMemberBalancesAndValidationNote()
    {
        var (service, db, _, _, _) = await CreateServiceAsync();

        var summary = await service.BuildFinancialSummaryAsync(2026, 5);

        Assert.False(summary.HasMeals);
        Assert.False(summary.HasExpenses);
        Assert.NotNull(summary.CalculationNote);
        var row = Assert.Single(summary.MemberBalances);
        Assert.Equal(0, row.TotalMeals);
        Assert.Equal(0m, row.MealCost);
        Assert.Equal(0m, row.Balance);

        await db.DisposeAsync();
    }

    private static async Task SeedExpenseAndMealAsync(
        MesslyDbContext db,
        Guid flatId,
        Guid userId,
        decimal expenseAmount,
        int mealCount)
    {
        var categoryId = db.ExpenseCategories.First(c => c.FlatId == flatId).Id;
        db.Expenses.Add(new Expense
        {
            FlatId = flatId,
            PaidByUserId = userId,
            ExpenseCategoryId = categoryId,
            Title = "Grocery",
            Amount = expenseAmount,
            ExpenseDate = new DateOnly(2026, 5, 10),
            CreatedAt = DateTime.UtcNow
        });
        db.Meals.Add(new Meal
        {
            FlatId = flatId,
            UserId = userId,
            MealDate = new DateOnly(2026, 5, 10),
            BreakfastCount = mealCount,
            LunchCount = 0,
            DinnerCount = 0,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    private static async Task<(BillingCalculationService Service, MesslyDbContext Db, Guid FlatId, Guid UserId, Guid CategoryId)> CreateServiceAsync()
    {
        var options = new DbContextOptionsBuilder<MesslyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new MesslyDbContext(options);
        var flatId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        db.AppUsers.Add(new User { Id = userId, FullName = "Test", Email = "t@test.com", CreatedAt = DateTime.UtcNow });
        db.AppRoles.Add(new Role { Id = roleId, Name = "Manager", RoleType = RoleType.Manager, CreatedAt = DateTime.UtcNow });
        db.Flats.Add(new Flat { Id = flatId, Name = "Test", CreatorId = userId, CreatedAt = DateTime.UtcNow });
        db.FlatMembers.Add(new FlatMember
        {
            Id = Guid.NewGuid(),
            FlatId = flatId,
            UserId = userId,
            RoleId = roleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        db.ExpenseCategories.Add(new ExpenseCategory { Id = categoryId, FlatId = flatId, Name = "Grocery", CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var memberRepo = new FlatMemberRepository(db);
        ITenantContext tenant = new TestTenantContext(flatId, RoleType.Manager, userId);
        var auth = new FlatAuthorizationService(tenant, memberRepo);

        var service = new BillingCalculationService(
            new ExpenseRepository(db),
            new DepositRepository(db),
            new MealRepository(db),
            memberRepo,
            new Repository<MonthlySummary>(db),
            new UnitOfWork(db),
            auth);

        return (service, db, flatId, userId, categoryId);
    }
}
