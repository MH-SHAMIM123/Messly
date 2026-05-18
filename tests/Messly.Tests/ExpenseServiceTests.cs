using Messly.Application.DTOs;
using Messly.Application.Services;
using Messly.Application.Validators;
using Messly.Domain.Entities;
using Messly.Domain.Enums;
using Messly.Infrastructure.Data;
using Messly.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Messly.Tests;

public class ExpenseServiceTests
{
    [Fact]
    public async Task GetCategoriesAsync_SeedsDefaultsWhenEmpty()
    {
        var (service, db, flatId, userId) = await CreateServiceAsync();
        var categories = await service.GetCategoriesAsync(flatId);
        Assert.True(categories.Count >= 5);
        Assert.Contains(categories, c => c.Name == "Grocery");
        await db.DisposeAsync();
    }

    [Fact]
    public async Task CreateExpenseAsync_AppearsInList()
    {
        var (service, db, flatId, userId) = await CreateServiceAsync();
        var categories = await service.GetCategoriesAsync(flatId);
        var id = await service.CreateExpenseAsync(new ExpenseUpsertDto
        {
            FlatId = flatId,
            Title = "Rice purchase",
            Amount = 500,
            ExpenseDate = new DateOnly(2026, 5, 10),
            PaidByUserId = userId,
            ExpenseCategoryId = categories[0].Id,
            ExpenseType = ExpenseType.Grocery
        });

        var list = await service.GetExpensesAsync(flatId);
        Assert.Contains(list, e => e.Id == id && e.Amount == 500);
        await db.DisposeAsync();
    }

    [Fact]
    public async Task DeleteExpenseAsync_SoftDeletes()
    {
        var (service, db, flatId, userId) = await CreateServiceAsync();
        var categories = await service.GetCategoriesAsync(flatId);
        var id = await service.CreateExpenseAsync(new ExpenseUpsertDto
        {
            FlatId = flatId,
            Title = "To delete",
            Amount = 100,
            ExpenseDate = DateOnly.FromDateTime(DateTime.Today),
            PaidByUserId = userId,
            ExpenseCategoryId = categories[0].Id,
            ExpenseType = ExpenseType.Other
        });

        await service.DeleteExpenseAsync(id);
        var list = await service.GetExpensesAsync(flatId);
        Assert.DoesNotContain(list, e => e.Id == id);
        await db.DisposeAsync();
    }

    private static async Task<(ExpenseService Service, MesslyDbContext Db, Guid FlatId, Guid UserId)> CreateServiceAsync()
    {
        var options = new DbContextOptionsBuilder<MesslyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new MesslyDbContext(options);
        var flatId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        db.AppUsers.Add(new User { Id = userId, FullName = "Payer", Email = "payer@test.com", CreatedAt = DateTime.UtcNow });
        db.Flats.Add(new Flat { Id = flatId, Name = "Flat", CreatorId = userId, CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var service = new ExpenseService(
            new ExpenseRepository(db),
            new ExpenseCategoryRepository(db),
            new UnitOfWork(db),
            new ExpenseUpsertDtoValidator());

        return (service, db, flatId, userId);
    }
}
