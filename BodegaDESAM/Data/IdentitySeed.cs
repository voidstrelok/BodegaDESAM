using Microsoft.AspNetCore.Identity;
using BodegaDESAM.Auth;

namespace BodegaDESAM.Data;

public static class IdentitySeed
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        var roles = new[] { AppRoles.Admin, AppRoles.Bodega, AppRoles.Lectura };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        await EnsureUserAsync(userManager, "rcontreras@desam", "Admin123", AppRoles.Admin);
        await EnsureUserAsync(userManager, "fcortes@desam", "Bodega123", AppRoles.Bodega);
        await EnsureUserAsync(userManager, "fespinosa@desam", "Lectura123", AppRoles.Lectura);

    }

    private static async Task EnsureUserAsync(UserManager<IdentityUser> userManager, string email, string password, string role)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var created = await userManager.CreateAsync(user, password);
            if (!created.Succeeded)
                return;
        }

        if (!await userManager.IsInRoleAsync(user, role))
            await userManager.AddToRoleAsync(user, role);
    }
}
