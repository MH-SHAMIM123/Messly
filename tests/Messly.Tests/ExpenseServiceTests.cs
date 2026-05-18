using Messly.Application.DTOs;
using Messly.Application.Interfaces.Security;
using Messly.Application.Services;
using Messly.Application.Services.Security;
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
        var (service, db, _, _) = await CreateServiceAsync();
        var categories = await service.GetCategoriesAsync();
        Assert.True(categories.Count >= 5);
        Assert.Contains(categories, c => c.Name == "Grocery");
        await db.DisposeAsync();
    }

    [Fact]
    public async Task CreateExpenseAsync_AppearsInList()
    {
        var (service, db, _, userId) = await CreateServiceAsync();
        var categories = await service.GetCategoriesAsync();
        var id = await service.CreateExpenseAsync(new ExpenseUpsertDto
        {
            Title = "Rice purchase",
            Amount = 500,
            ExpenseDate = new DateOnly(2026, 5, 10),
            PaidByUserId = userId,
            ExpenseCategoryId = categories[0].Id,
            ExpenseType = ExpenseType.Grocery
        });

        var list = await service.GetExpensesAsync();
        Assert.Contains(list, e => e.Id == id && e.Amount == 500);
        await db.DisposeAsync();
    }

    [Fact]
    public async Task DeleteExpenseAsync_SoftDeletes()
    {
        var (service, db, _, userId) = await CreateServiceAsync();
        var categories = await service.GetCategoriesAsync();
        var id = await service.CreateExpenseAsync(new ExpenseUpsertDto
        {
            Title = "To delete",
            Amount = 100,
            ExpenseDate = DateOnly.FromDateTime(DateTime.Today),
            PaidByUserId = userId,
            ExpenseCategoryId = categories[0].Id,
            ExpenseType = ExpenseType.Other
        });

        await service.DeleteExpenseAsync(id);
        var list = await service.GetExpensesAsync();
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
        var managerRoleId = Guid.NewGuid();

        db.AppUsers.Add(new User { Id = userId, FullName = "Payer", Email = "payer@test.com", CreatedAt = DateTime.UtcNow });
        db.AppRoles.Add(new Role { Id = managerRoleId, Name = "Manager", RoleType = RoleType.Manager, CreatedAt = DateTime.UtcNow });
        db.Flats.Add(new Flat { Id = flatId, Name = "Flat", CreatorId = userId, CreatedAt = DateTime.UtcNow });
        db.FlatMembers.Add(new FlatMember
        {
            Id = Guid.NewGuid(),
            FlatId = flatId,
            UserId = userId,
            RoleId = managerRoleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var memberRepo = new FlatMemberRepository(db);
        ITenantContext tenant = new TestTenantContext(flatId, RoleType.Manager, userId);
        var auth = new FlatAuthorizationService(tenant, memberRepo);

        var service = new ExpenseService(
            new ExpenseRepository(db),
            new ExpenseCategoryRepository(db),
            new UnitOfWork(db),
            new ExpenseUpsertDtoValidator(),
            auth);

        return (service, db, flatId, userId);
    }
}
