using Microsoft.AspNetCore.Identity;

namespace BodegaDESAM.Services;

public sealed class UserDisplayNameService
{
    private readonly UserManager<IdentityUser> _userManager;

    public UserDisplayNameService(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<string?> GetNombreCompletoAsync(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return null;

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return null;

        var claims = await _userManager.GetClaimsAsync(user);
        var nombre = claims.FirstOrDefault(c => c.Type == "given_name")?.Value;
        var apellido = claims.FirstOrDefault(c => c.Type == "family_name")?.Value;
        var nombreCompleto = string.Join(" ", new[] { nombre, apellido }
            .Where(valor => !string.IsNullOrWhiteSpace(valor))
            .Select(valor => valor!.Trim()));

        return string.IsNullOrWhiteSpace(nombreCompleto) ? null : nombreCompleto;
    }

    // Conserva compatibilidad con los consumidores existentes.
    public Task<string?> GetNombreAsync(string? userId) => GetNombreCompletoAsync(userId);
}
