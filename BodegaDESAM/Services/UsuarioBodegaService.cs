using Microsoft.EntityFrameworkCore;

namespace BodegaDESAM.Services;

public sealed class UsuarioBodegaService
{
    private readonly IDbContextFactory<PostgresDataContext> _factory;
    private readonly AuditService _audit;
    public UsuarioBodegaService(IDbContextFactory<PostgresDataContext> factory, AuditService audit) { _factory = factory; _audit = audit; }

    public async Task<HashSet<int>> GetAsignadasAsync(string idUsuario)
    {
        using var db = _factory.CreateDbContext();
        return (await db.UsuarioBodega.Where(x => x.IdUsuario == idUsuario).Select(x => x.IdBodega).ToListAsync()).ToHashSet();
    }

    public async Task ReemplazarAsync(string idUsuario, IEnumerable<int> idsBodega)
    {
        var ids = idsBodega.Where(x => x > 0).Distinct().ToHashSet();
        using var db = _factory.CreateDbContext();
        var validas = await db.Bodega.Where(b => b.Activa && ids.Contains(b.Id)).Select(b => b.Id).ToListAsync();
        var anteriores = await db.UsuarioBodega.Where(x => x.IdUsuario == idUsuario).ToListAsync();
        var anterioresIds = anteriores.Select(x => x.IdBodega).ToHashSet();
        db.UsuarioBodega.RemoveRange(anteriores);
        db.UsuarioBodega.AddRange(validas.Select(id => new UsuarioBodega { IdUsuario = idUsuario, IdBodega = id }));
        await db.SaveChangesAsync();
        foreach (var id in validas.Except(anterioresIds)) await _audit.RegistrarActualAsync(AuditAcciones.Asignar, AuditEntidades.UsuarioBodega, id, new { UsuarioAfectadoId = idUsuario }, id);
        foreach (var id in anterioresIds.Except(validas)) await _audit.RegistrarActualAsync(AuditAcciones.Quitar, AuditEntidades.UsuarioBodega, id, new { UsuarioAfectadoId = idUsuario }, id);
    }
}
