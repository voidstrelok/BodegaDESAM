using Microsoft.EntityFrameworkCore;

namespace BodegaDESAM.Services;

public class BodegaService
{
    private readonly IDbContextFactory<PostgresDataContext> _factory;

    public BodegaService(IDbContextFactory<PostgresDataContext> factory)
    {
        _factory = factory;
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
        }
    }
}
