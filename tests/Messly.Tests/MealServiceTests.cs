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

public class MealServiceTests
{
    [Fact]
    public async Task SaveDailyEntriesAsync_PersistsAndReloads()
    {
        var (service, db, _, userId) = await CreateServiceAsync();
        var date = new DateOnly(2026, 5, 18);
        var entries = new List<MealEntryDto>
        {
            new()
            {
                UserId = userId,
                MemberName = "Member",
                BreakfastCount = 1,
                LunchCount = 2,
                DinnerCount = 1
            }
        };

        await service.SaveDailyEntriesAsync(date, entries);

        var loaded = await service.GetMealEntriesByDateAsync(date);
        var row = Assert.Single(loaded);
        Assert.Equal(1, row.BreakfastCount);
        Assert.Equal(2, row.LunchCount);
        Assert.Equal(1, row.DinnerCount);

        await db.DisposeAsync();
    }

    [Fact]
    public async Task SaveDailyEntriesAsync_UpdatesExistingByUserAndDate()
    {
        var (service, db, _, userId) = await CreateServiceAsync();
        var date = new DateOnly(2026, 5, 18);
        var entry = new MealEntryDto
        {
            UserId = userId,
            MemberName = "Member",
            BreakfastCount = 1,
            LunchCount = 1,
            DinnerCount = 1
        };

        await service.SaveDailyEntriesAsync(date, [entry]);
        entry.BreakfastCount = 3;
        entry.LunchCount = 0;
        entry.DinnerCount = 2;
        await service.SaveDailyEntriesAsync(date, [entry]);

        var loaded = await service.GetMealEntriesByDateAsync(date);
        var row = Assert.Single(loaded);
        Assert.Equal(3, row.BreakfastCount);
        Assert.Equal(0, row.LunchCount);
        Assert.Equal(2, row.DinnerCount);

        await db.DisposeAsync();
    }

    [Fact]
    public async Task SaveDailyEntriesAsync_RejectsInvalidCounts()
    {
        var (service, db, _, userId) = await CreateServiceAsync();
        var date = new DateOnly(2026, 5, 18);

        await Assert.ThrowsAsync<Messly.Application.Common.BusinessException>(() =>
            service.SaveDailyEntriesAsync(date,
            [
                new MealEntryDto
                {
                    UserId = userId,
                    BreakfastCount = 4,
                    LunchCount = 0,
                    DinnerCount = 0
                }
            ]));

        await db.DisposeAsync();
    }

    [Fact]
    public async Task GetMealTotalsByMonthAsync_AggregatesPerMember()
    {
        var (service, db, _, userId) = await CreateServiceAsync();
        var date1 = new DateOnly(2026, 5, 1);
        var date2 = new DateOnly(2026, 5, 2);

        await service.SaveDailyEntriesAsync(date1,
        [
            new MealEntryDto { UserId = userId, BreakfastCount = 1, LunchCount = 1, DinnerCount = 1 }
        ]);
        await service.SaveDailyEntriesAsync(date2,
        [
            new MealEntryDto { UserId = userId, BreakfastCount = 2, LunchCount = 0, DinnerCount = 1 }
        ]);

        var summary = await service.GetMealTotalsByMonthAsync(2026, 5);
        var row = Assert.Single(summary);
        Assert.Equal(3, row.TotalBreakfast);
        Assert.Equal(1, row.TotalLunch);
        Assert.Equal(2, row.TotalDinner);
        Assert.Equal(6, row.GrandTotal);

        await db.DisposeAsync();
    }

    private static async Task<(MealService Service, MesslyDbContext Db, Guid FlatId, Guid UserId)> CreateServiceAsync()
    {
        var options = new DbContextOptionsBuilder<MesslyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new MesslyDbContext(options);
        var flatId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        db.AppUsers.Add(new User { Id = userId, FullName = "Member", Email = "m@test.com", CreatedAt = DateTime.UtcNow });
        db.AppRoles.Add(new Role { Id = roleId, Name = "Manager", RoleType = RoleType.Manager, CreatedAt = DateTime.UtcNow });
        db.Flats.Add(new Flat { Id = flatId, Name = "Flat", CreatorId = userId, CreatedAt = DateTime.UtcNow });
        db.FlatMembers.Add(new FlatMember
        {
            Id = Guid.NewGuid(),
            FlatId = flatId,
            UserId = userId,
            RoleId = roleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var memberRepo = new FlatMemberRepository(db);
        ITenantContext tenant = new TestTenantContext(flatId, RoleType.Manager, userId);
        var auth = new FlatAuthorizationService(tenant, memberRepo);

        var service = new MealService(
            new MealRepository(db),
            memberRepo,
            new UnitOfWork(db),
            new MealEntryDtoValidator(),
            auth);

        return (service, db, flatId, userId);
    }
}
