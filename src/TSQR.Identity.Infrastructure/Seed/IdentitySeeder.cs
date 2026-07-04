using Microsoft.AspNetCore.Identity;

namespace TSQR.Identity.Infrastructure.Seed;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider sp)
    {
        var roleManager = sp.GetRequiredService<RoleManager<Role>>();

        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new Role("Admin"));

        if (!await roleManager.RoleExistsAsync("Member"))
            await roleManager.CreateAsync(new Role("Member"));

        if (!await roleManager.RoleExistsAsync("Repairman"))
            await roleManager.CreateAsync(new Role("Repairman"));

        if (!await roleManager.RoleExistsAsync("LocationCoordinator"))
            await roleManager.CreateAsync(new Role("LocationCoordinator"));
    }
}
