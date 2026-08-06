using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BodegaDESAM.Auth;
using System.Security.Claims;

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

        await EnsureUserAsync(userManager, "rcontreras@desam", "Admin123", AppRoles.Admin, "Ricardo", "Contreras", "Cortés");
        await EnsureUserAsync(userManager, "fcortes@desam", "Bodega123", AppRoles.Bodega, "Francisco", "Cortés", "Castillo");
        await EnsureUserAsync(userManager, "fespinosa@desam", "Lectura123", AppRoles.Lectura, "Fernanda", "Espinosa", "Aguirre");

        //Seed incial, establecimientos, bodega
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<PostgresDataContext>>();
        await using var db = await factory.CreateDbContextAsync();

        if (await db.Bodega.AnyAsync() || await db.Establecimiento.AnyAsync())
            return;
        
        // ============ BODEGA CENTRAL (única) ============
        var bodegaCentral = new Bodega
        {
            Codigo = "BOD-CENTRAL",
            Nombre = "Bodega Central DESAM",
            Direccion = "Avenida Ferroviaria, Monte Patria, Región de Coquimbo",
            EsPrincipal = true,
            Activa = true
        };
        db.Bodega.Add(bodegaCentral);
        await db.SaveChangesAsync();

        var bodegas = new[] { bodegaCentral };
       
        // ============ ESTABLECIMIENTOS ============
        var establecimientos = new[]
        {
            new Establecimiento { Nombre = "Centro de Salud Familiar Monte Patria" },
            new Establecimiento { Nombre = "Centro de Salud Familiar Carén" },
            new Establecimiento { Nombre = "Centro de Salud Familiar El Palqui" },
            new Establecimiento { Nombre = "Centro de Salud Familiar Chañaral Alto" },
            new Establecimiento { Nombre = "Departamento de Salud" }
        };
        db.Establecimiento.AddRange(establecimientos);
        await db.SaveChangesAsync();

    }

    private static async Task EnsureUserAsync(
        UserManager<IdentityUser> userManager,
        string email,
        string password,
        string role,
        string nombre,
        string apellido,
        string apellidoMaterno)
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

        var claims = await userManager.GetClaimsAsync(user);
        await UpsertClaimAsync(userManager, user, claims, "given_name", nombre);
        await UpsertClaimAsync(userManager, user, claims, "family_name", apellido);
        await UpsertClaimAsync(userManager, user, claims, "maternal_family_name", apellidoMaterno);
    }

    private static async Task UpsertClaimAsync(
        UserManager<IdentityUser> userManager,
        IdentityUser user,
        IList<Claim> claims,
        string type,
        string value)
    {
        var existing = claims.FirstOrDefault(claim => claim.Type == type);
        var updated = new Claim(type, value);

        if (existing is null)
            await userManager.AddClaimAsync(user, updated);
        else if (existing.Value != value)
            await userManager.ReplaceClaimAsync(user, existing, updated);
    }
}
