using Messly.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Messly.Infrastructure.Identity;

public static class IdentityDataSeeder
{
    public const string DefaultPassword = "Manager@123";

    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        if (!await roleManager.RoleExistsAsync("Manager"))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid> { Name = "Manager" });
        }

        var managerId = DevDataSeeder.ManagerUserId;
        var existing = await userManager.FindByEmailAsync("manager@messly.local");
        if (existing is null)
        {
            var user = new ApplicationUser
            {
                Id = managerId,
                UserName = "manager@messly.local",
                Email = "manager@messly.local",
                EmailConfirmed = true,
                DomainUserId = managerId
            };
            await userManager.CreateAsync(user, DefaultPassword);
            await userManager.AddToRoleAsync(user, "Manager");
        }

        var flatId = Guid.TryParse(configuration["Messly:DefaultFlatId"], out var id)
            ? id
            : DevDataSeeder.DefaultFlatId;

        var db = services.GetRequiredService<MesslyDbContext>();
        var hasMembership = await db.FlatMembers.AnyAsync(fm =>
            fm.UserId == managerId && fm.FlatId == flatId);
        if (!hasMembership)
        {
            await DevDataSeeder.SeedAsync(db, configuration);
        }
    }
}
