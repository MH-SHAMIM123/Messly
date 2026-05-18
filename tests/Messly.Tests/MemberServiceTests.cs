using Messly.Application.DTOs;
using Messly.Application.Services;
using Messly.Application.Validators;
using Messly.Domain.Entities;
using Messly.Domain.Enums;
using Messly.Infrastructure.Data;
using Messly.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Messly.Tests;

public class MemberServiceTests
{
    [Fact]
    public async Task CreateMemberAsync_AddsUserAndFlatMember()
    {
        var (service, db, flatId) = await CreateServiceAsync();
        var id = await service.CreateMemberAsync(new MemberUpsertDto
        {
            FlatId = flatId,
            FullName = "Rahim",
            Email = "rahim@test.com",
            RoleType = RoleType.Member
        });

        Assert.NotEqual(Guid.Empty, id);
        var members = await service.GetMembersAsync(flatId);
        Assert.Contains(members, m => m.Email == "rahim@test.com" && m.FullName == "Rahim");
        await db.DisposeAsync();
    }

    [Fact]
    public async Task UpdateMemberAsync_UpdatesUserDetails()
    {
        var (service, db, flatId) = await CreateServiceAsync();
        var id = await service.CreateMemberAsync(new MemberUpsertDto
        {
            FlatId = flatId,
            FullName = "Karim",
            Email = "karim@test.com",
            RoleType = RoleType.Member
        });

        await service.UpdateMemberAsync(new MemberUpsertDto
        {
            Id = id,
            FlatId = flatId,
            FullName = "Karim Updated",
            Email = "karim@test.com",
            RoleType = RoleType.Member
        });

        var member = await service.GetMemberAsync(id);
        Assert.Equal("Karim Updated", member!.FullName);
        await db.DisposeAsync();
    }

    [Fact]
    public async Task DeleteMemberAsync_SoftDeletesFlatMember()
    {
        var (service, db, flatId) = await CreateServiceAsync();
        var id = await service.CreateMemberAsync(new MemberUpsertDto
        {
            FlatId = flatId,
            FullName = "To Delete",
            Email = "delete@test.com",
            RoleType = RoleType.Member
        });

        await service.DeleteMemberAsync(id);

        var members = await service.GetMembersAsync(flatId);
        Assert.DoesNotContain(members, m => m.Id == id);
        await db.DisposeAsync();
    }

    private static async Task<(MemberService Service, MesslyDbContext Db, Guid FlatId)> CreateServiceAsync()
    {
        var options = new DbContextOptionsBuilder<MesslyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new MesslyDbContext(options);
        var flatId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var managerRoleId = Guid.Parse("11111111-1111-1111-1111-111111111101");
        var memberRoleId = Guid.Parse("11111111-1111-1111-1111-111111111102");

        db.AppUsers.Add(new User { Id = creatorId, FullName = "Creator", Email = "c@test.com", CreatedAt = DateTime.UtcNow });
        db.AppRoles.Add(new Role { Id = managerRoleId, Name = "Manager", RoleType = RoleType.Manager, CreatedAt = DateTime.UtcNow });
        db.AppRoles.Add(new Role { Id = memberRoleId, Name = "Member", RoleType = RoleType.Member, CreatedAt = DateTime.UtcNow });
        db.Flats.Add(new Flat { Id = flatId, Name = "Test Flat", CreatorId = creatorId, CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var service = new MemberService(
            new FlatMemberRepository(db),
            new UserRepository(db),
            new RoleRepository(db),
            new UnitOfWork(db),
            new MemberUpsertDtoValidator());

        return (service, db, flatId);
    }
}
