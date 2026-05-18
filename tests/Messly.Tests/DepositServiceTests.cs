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

public class DepositServiceTests
{
    [Fact]
    public async Task CreateDepositAsync_AppearsInList()
    {
        var (service, db, _, userId) = await CreateServiceAsync();
        var id = await service.CreateDepositAsync(new DepositUpsertDto
        {
            UserId = userId,
            Amount = 1500,
            DepositDate = new DateOnly(2026, 5, 12)
        });

        var list = await service.GetDepositsAsync();
        Assert.Contains(list, d => d.Id == id && d.Amount == 1500);
        await db.DisposeAsync();
    }

    [Fact]
    public async Task UpdateDepositAsync_UpdatesAmount()
    {
        var (service, db, _, userId) = await CreateServiceAsync();
        var id = await service.CreateDepositAsync(new DepositUpsertDto
        {
            UserId = userId,
            Amount = 1000,
            DepositDate = DateOnly.FromDateTime(DateTime.Today)
        });

        await service.UpdateDepositAsync(new DepositUpsertDto
        {
            Id = id,
            UserId = userId,
            Amount = 2000,
            DepositDate = DateOnly.FromDateTime(DateTime.Today)
        });

        var deposit = await service.GetDepositAsync(id);
        Assert.Equal(2000, deposit!.Amount);
        await db.DisposeAsync();
    }

    [Fact]
    public async Task DeleteDepositAsync_SoftDeletes()
    {
        var (service, db, _, userId) = await CreateServiceAsync();
        var id = await service.CreateDepositAsync(new DepositUpsertDto
        {
            UserId = userId,
            Amount = 500,
            DepositDate = DateOnly.FromDateTime(DateTime.Today)
        });

        await service.DeleteDepositAsync(id);
        var list = await service.GetDepositsAsync();
        Assert.DoesNotContain(list, d => d.Id == id);
        await db.DisposeAsync();
    }

    private static async Task<(DepositService Service, MesslyDbContext Db, Guid FlatId, Guid UserId)> CreateServiceAsync()
    {
        var options = new DbContextOptionsBuilder<MesslyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new MesslyDbContext(options);
        var flatId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var managerRoleId = Guid.NewGuid();

        db.AppUsers.Add(new User { Id = userId, FullName = "Member", Email = "m@test.com", CreatedAt = DateTime.UtcNow });
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

        var service = new DepositService(
            new DepositRepository(db),
            new UnitOfWork(db),
            new DepositUpsertDtoValidator(),
            auth);

        return (service, db, flatId, userId);
    }
}
