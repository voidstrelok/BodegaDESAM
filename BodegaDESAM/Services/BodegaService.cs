using Microsoft.EntityFrameworkCore;

namespace BodegaDESAM.Services;

public class BodegaService
{
    private readonly IDbContextFactory<PostgresDataContext> _factory;
    private readonly AuditService _audit;

    public BodegaService(IDbContextFactory<PostgresDataContext> factory, AuditService audit)
    {
        _factory = factory;
        _audit = audit;
    }

    public async Task<List<Bodega>> GetAllAsync()
    {
        using var db = _factory.CreateDbContext();
        return await db.Bodega
            .AsNoTracking()
            .Where(b => b.Activa)
            .OrderByDescending(b => b.EsPrincipal)
            .ThenBy(b => b.Nombre)
            .ToListAsync();
    }

    public async Task<PagedResult<Bodega>> GetPageAsync(PageRequest request, CancellationToken cancellationToken = default)
    {
        using var db = _factory.CreateDbContext();
        var query = db.Bodega.AsNoTracking().Where(b => b.Activa).AsQueryable();
        int? id = int.TryParse(request.Search, out var parsedId) ? parsedId : null;
        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(b =>
                EF.Functions.ILike(b.Codigo ?? string.Empty, $"%{request.Search}%") ||
                EF.Functions.ILike(b.Nombre, $"%{request.Search}%") ||
                EF.Functions.ILike(b.Direccion ?? string.Empty, $"%{request.Search}%") ||
                (id.HasValue && b.Id == id.Value));
        return await query.OrderByDescending(b => b.EsPrincipal).ThenBy(b => b.Nombre).ThenBy(b => b.Id).ToPagedAsync(request, cancellationToken);
    }

    public async Task<Bodega?> GetByIdAsync(int id)
    {
        using var db = _factory.CreateDbContext();
        return await db.Bodega
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Bodega?> GetPrincipalAsync()
    {
        using var db = _factory.CreateDbContext();
        return await db.Bodega
            .AsNoTracking()
            .Where(b => b.Activa && b.EsPrincipal)
            .FirstOrDefaultAsync();
    }

    public async Task CreateAsync(Bodega bodega)
    {
        using var db = _factory.CreateDbContext();
        
        // Si se marca como principal, desmarcar las demás
        if (bodega.EsPrincipal)
        {
            var otras = await db.Bodega.Where(b => b.EsPrincipal).ToListAsync();
            foreach (var otra in otras)
            {
                otra.EsPrincipal = false;
            }
        }

        db.Bodega.Add(bodega);
        await db.SaveChangesAsync();
        await _audit.RegistrarActualAsync(AuditAcciones.Crear, AuditEntidades.Bodega, bodega.Id, new { bodega.Codigo, bodega.Nombre, bodega.EsPrincipal }, bodega.Id);
    }

    public async Task UpdateAsync(Bodega bodega)
    {
        using var db = _factory.CreateDbContext();
        
        // Si se marca como principal, desmarcar las demás
        if (bodega.EsPrincipal)
        {
            var otras = await db.Bodega.Where(b => b.EsPrincipal && b.Id != bodega.Id).ToListAsync();
            foreach (var otra in otras)
            {
                otra.EsPrincipal = false;
            }
        }

        db.Bodega.Update(bodega);
        await db.SaveChangesAsync();
        await _audit.RegistrarActualAsync(AuditAcciones.Editar, AuditEntidades.Bodega, bodega.Id, new { bodega.Codigo, bodega.Nombre, bodega.EsPrincipal, bodega.Activa }, bodega.Id);
    }

    public async Task DeleteAsync(int id)
    {
        using var db = _factory.CreateDbContext();
        var bodega = await db.Bodega.FindAsync(id);
        if (bodega != null)
        {
            // En lugar de eliminar, desactivar
            bodega.Activa = false;
            await db.SaveChangesAsync();
            await _audit.RegistrarActualAsync(AuditAcciones.Desactivar, AuditEntidades.Bodega, bodega.Id, new { bodega.Codigo, bodega.Nombre }, bodega.Id);
        }
    }
}
