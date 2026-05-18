using Messly.Application.Services;
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
        var options = new DbContextOptionsBuilder<MesslyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new MesslyDbContext(options);
        var flatId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        db.AppUsers.Add(new User { Id = userId, FullName = "Test", Email = "t@test.com", CreatedAt = DateTime.UtcNow });
        db.AppRoles.Add(new Role { Id = roleId, Name = "Member", RoleType = RoleType.Member, CreatedAt = DateTime.UtcNow });
        db.Flats.Add(new Flat { Id = flatId, Name = "Test", CreatorId = userId, CreatedAt = DateTime.UtcNow });
        db.ExpenseCategories.Add(new ExpenseCategory { Id = categoryId, FlatId = flatId, Name = "Grocery", CreatedAt = DateTime.UtcNow });
        db.Expenses.Add(new Expense
        {
            FlatId = flatId,
            PaidByUserId = userId,
            ExpenseCategoryId = categoryId,
            Title = "Grocery",
            Amount = 3000,
            ExpenseDate = new DateOnly(2026, 5, 10),
            CreatedAt = DateTime.UtcNow
        });
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

        var service = new BillingCalculationService(
            new ExpenseRepository(db),
            new DepositRepository(db),
            new MealRepository(db),
            new FlatMemberRepository(db),
            new Repository<MonthlySummary>(db),
            new UnitOfWork(db));

        var rate = await service.CalculateMealRateAsync(flatId, 2026, 5);

        Assert.Equal(1000m, rate);
    }
}
