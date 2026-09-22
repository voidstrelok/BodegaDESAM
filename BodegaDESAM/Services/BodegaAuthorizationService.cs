using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BodegaDESAM.Services;

public sealed class BodegaAuthorizationService
{
    private readonly IDbContextFactory<PostgresDataContext> _factory;
    public BodegaAuthorizationService(IDbContextFactory<PostgresDataContext> factory) => _factory = factory;

    public async Task<bool> PuedeAccederAsync(ClaimsPrincipal user, int idBodega)
    {
        if (user.IsInRole(Auth.AppRoles.Admin)) return true;
        var idUsuario = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(idUsuario)) return false;
        using var db = _factory.CreateDbContext();
        return await db.UsuarioBodega.AnyAsync(x => x.IdUsuario == idUsuario && x.IdBodega == idBodega && x.Bodega.Activa);
    }
}
