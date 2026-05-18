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

public class MemberServiceTests
{
    [Fact]
    public async Task CreateMemberAsync_AddsUserAndFlatMember()
    {
        var (service, db, flatId) = await CreateServiceAsync();
        var id = await service.CreateMemberAsync(new MemberUpsertDto
        {
            FullName = "Rahim",
            Email = "rahim@test.com",
            RoleType = RoleType.Member
        });

        Assert.NotEqual(Guid.Empty, id);
        var members = await service.GetMembersAsync();
        Assert.Contains(members, m => m.Email == "rahim@test.com" && m.FullName == "Rahim");
        await db.DisposeAsync();
    }

    [Fact]
    public async Task UpdateMemberAsync_UpdatesUserDetails()
    {
        var (service, db, flatId) = await CreateServiceAsync();
        var id = await service.CreateMemberAsync(new MemberUpsertDto
        {
            FullName = "Karim",
            Email = "karim@test.com",
            RoleType = RoleType.Member
        });

        await service.UpdateMemberAsync(new MemberUpsertDto
        {
            Id = id,
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
            FullName = "To Delete",
            Email = "delete@test.com",
            RoleType = RoleType.Member
        });

        await service.DeleteMemberAsync(id);

        var members = await service.GetMembersAsync();
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
        db.FlatMembers.Add(new FlatMember
        {
            Id = Guid.NewGuid(),
            FlatId = flatId,
            UserId = creatorId,
            RoleId = managerRoleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var memberRepo = new FlatMemberRepository(db);
        ITenantContext tenant = new TestTenantContext(flatId, RoleType.Manager, creatorId);
        var auth = new FlatAuthorizationService(tenant, memberRepo);

        var service = new MemberService(
            memberRepo,
            new UserRepository(db),
            new RoleRepository(db),
            new UnitOfWork(db),
            new MemberUpsertDtoValidator(),
            auth);

        return (service, db, flatId);
    }
}
