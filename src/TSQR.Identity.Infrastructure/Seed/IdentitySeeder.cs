using Microsoft.AspNetCore.Identity;

namespace TSQR.Identity.Infrastructure.Seed;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider sp)
    {
        var userManager = sp.GetRequiredService<UserManager<AppUser>>();
        var roleManager = sp.GetRequiredService<RoleManager<Role>>();

        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new Role("Admin"));
        }

        if (!await roleManager.RoleExistsAsync("Member"))
        {
            await roleManager.CreateAsync(new Role("Member"));
        }

        var admin = await userManager.FindByEmailAsync("admin@tsqr.local");
        if (admin is null)
        {
            admin = new AppUser
            {
                UserName = "admin",
                Email = "admin@tsqr.local",
                FirstName = "System",
                LastName = "Admin"
            };
            var result = await userManager.CreateAsync(admin, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}
