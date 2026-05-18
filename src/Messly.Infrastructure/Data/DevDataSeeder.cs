using Messly.Domain.Entities;
using Messly.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Messly.Infrastructure.Data;

public static class DevDataSeeder
{
    public static readonly Guid DefaultFlatId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public static readonly Guid ManagerUserId = Guid.Parse("22222222-2222-2222-2222-222222222201");
    public static readonly Guid ManagerFlatMemberId = Guid.Parse("33333333-3333-3333-3333-333333333301");
    public static readonly Guid ManagerRoleId = Guid.Parse("11111111-1111-1111-1111-111111111101");
    public static readonly Guid MemberRoleId = Guid.Parse("11111111-1111-1111-1111-111111111102");

    public static async Task SeedAsync(MesslyDbContext db, IConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var flatId = Guid.TryParse(configuration["Messly:DefaultFlatId"], out var configuredFlatId)
            ? configuredFlatId
            : DefaultFlatId;

        if (!await db.Flats.AnyAsync(f => f.Id == flatId, cancellationToken))
        {
            var managerExists = await db.AppUsers.AnyAsync(u => u.Id == ManagerUserId, cancellationToken);
            if (!managerExists)
            {
                db.AppUsers.Add(new User
                {
                    Id = ManagerUserId,
                    FullName = "Mess Manager",
                    Email = "manager@messly.local",
                    Phone = null,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            db.Flats.Add(new Flat
            {
                Id = flatId,
                Name = "Demo Mess Flat",
                Address = "Dhaka, Bangladesh",
                Description = "Development seed flat",
                DefaultMealRate = 0,
                BillingDayOfMonth = 1,
                CreatorId = ManagerUserId,
                CreatedAt = DateTime.UtcNow
            });

            if (!await db.FlatMembers.AnyAsync(fm => fm.Id == ManagerFlatMemberId, cancellationToken))
            {
                db.FlatMembers.Add(new FlatMember
                {
                    Id = ManagerFlatMemberId,
                    FlatId = flatId,
                    UserId = ManagerUserId,
                    RoleId = ManagerRoleId,
                    JoinedAt = DateTime.UtcNow,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            var categories = new[]
            {
                ("Grocery", Guid.Parse("44444444-4444-4444-4444-444444444401")),
                ("Utility", Guid.Parse("44444444-4444-4444-4444-444444444402")),
                ("Gas", Guid.Parse("44444444-4444-4444-4444-444444444403")),
                ("Cook Salary", Guid.Parse("44444444-4444-4444-4444-444444444404")),
                ("Other", Guid.Parse("44444444-4444-4444-4444-444444444405"))
            };

            foreach (var (name, id) in categories)
            {
                db.ExpenseCategories.Add(new ExpenseCategory
                {
                    Id = id,
                    FlatId = flatId,
                    Name = name,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
