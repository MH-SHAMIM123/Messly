using Messly.Application.DTOs;
using Messly.Application.Services;
using Messly.Application.Validators;
using Messly.Domain.Entities;
using Messly.Infrastructure.Data;
using Messly.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Messly.Tests;

public class DepositServiceTests
{
    [Fact]
    public async Task CreateDepositAsync_AppearsInList()
    {
        var (service, db, flatId, userId) = await CreateServiceAsync();
        var id = await service.CreateDepositAsync(new DepositUpsertDto
        {
            FlatId = flatId,
            UserId = userId,
            Amount = 1500,
            DepositDate = new DateOnly(2026, 5, 12)
        });

        var list = await service.GetDepositsAsync(flatId);
        Assert.Contains(list, d => d.Id == id && d.Amount == 1500);
        await db.DisposeAsync();
    }

    [Fact]
    public async Task UpdateDepositAsync_UpdatesAmount()
    {
        var (service, db, flatId, userId) = await CreateServiceAsync();
        var id = await service.CreateDepositAsync(new DepositUpsertDto
        {
            FlatId = flatId,
            UserId = userId,
            Amount = 1000,
            DepositDate = DateOnly.FromDateTime(DateTime.Today)
        });

        await service.UpdateDepositAsync(new DepositUpsertDto
        {
            Id = id,
            FlatId = flatId,
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
        var (service, db, flatId, userId) = await CreateServiceAsync();
        var id = await service.CreateDepositAsync(new DepositUpsertDto
        {
            FlatId = flatId,
            UserId = userId,
            Amount = 500,
            DepositDate = DateOnly.FromDateTime(DateTime.Today)
        });

        await service.DeleteDepositAsync(id);
        var list = await service.GetDepositsAsync(flatId);
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

        db.AppUsers.Add(new User { Id = userId, FullName = "Member", Email = "m@test.com", CreatedAt = DateTime.UtcNow });
        db.Flats.Add(new Flat { Id = flatId, Name = "Flat", CreatorId = userId, CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var service = new DepositService(
            new DepositRepository(db),
            new UnitOfWork(db),
            new DepositUpsertDtoValidator());

        return (service, db, flatId, userId);
    }
}
