using Microsoft.EntityFrameworkCore;

namespace BodegaDESAM.Services;

public sealed class AlertaStockService
{
    private readonly IDbContextFactory<PostgresDataContext> _factory;
    private readonly AuditService _audit;
    public AlertaStockService(IDbContextFactory<PostgresDataContext> factory, AuditService audit) { _factory = factory; _audit = audit; }

    public async Task<List<AlertaStock>> GetByBodegaAsync(int idBodega)
    {
        using var db = _factory.CreateDbContext();
            return await db.AlertaStock.AsNoTracking().Include(x => x.Producto).Where(x => x.IdBodega == idBodega).OrderBy(x => x.Producto.Nombre).ToListAsync();
    }

    public async Task<PagedResult<AlertaStock>> GetPageAsync(int idBodega, PageRequest request, CancellationToken cancellationToken = default)
    {
        using var db = _factory.CreateDbContext();
        var query = db.AlertaStock.AsNoTracking().Include(x => x.Producto).Where(x => x.IdBodega == idBodega).AsQueryable();
        int? id = int.TryParse(request.Search, out var parsedId) ? parsedId : null;
        int? stock = int.TryParse(request.Search, out var parsedStock) ? parsedStock : null;
        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(x => EF.Functions.ILike(x.Producto.Nombre, $"%{request.Search}%") ||
                (id.HasValue && x.Id == id.Value) || (stock.HasValue && x.StockMinimo == stock.Value));
        return await query.OrderBy(x => x.Producto.Nombre).ThenBy(x => x.Id).ToPagedAsync(request, cancellationToken);
    }
    public async Task<AlertaStock?> GetByIdAsync(int id, int idBodega)
    {
        using var db = _factory.CreateDbContext();
        return await db.AlertaStock.AsNoTracking().Include(x => x.Producto).FirstOrDefaultAsync(x => x.Id == id && x.IdBodega == idBodega);
    }
    public async Task CreateAsync(AlertaStock alerta)
    {
        Validar(alerta); using var db = _factory.CreateDbContext();
        if (!await db.Bodega.AnyAsync(b => b.Id == alerta.IdBodega && b.Activa)) throw new InvalidOperationException("La bodega activa no está disponible.");
        if (!await db.Producto.AnyAsync(p => p.Id == alerta.IdProducto)) throw new InvalidOperationException("El producto seleccionado no existe.");
        if (await db.AlertaStock.AnyAsync(x => x.IdBodega == alerta.IdBodega && x.IdProducto == alerta.IdProducto)) throw new InvalidOperationException("Ya existe una alerta para este producto en la bodega activa.");
        db.AlertaStock.Add(alerta); await db.SaveChangesAsync();
        await _audit.RegistrarActualAsync(AuditAcciones.Crear, AuditEntidades.AlertaStock, alerta.Id, new { alerta.IdProducto, alerta.StockMinimo }, alerta.IdBodega);
    }
    public async Task UpdateAsync(AlertaStock alerta, int idBodega)
    {
        Validar(alerta); using var db = _factory.CreateDbContext();
        var actual = await db.AlertaStock.FirstOrDefaultAsync(x => x.Id == alerta.Id && x.IdBodega == idBodega) ?? throw new InvalidOperationException("La alerta no existe o no pertenece a la bodega activa.");
        actual.StockMinimo = alerta.StockMinimo; await db.SaveChangesAsync();
        await _audit.RegistrarActualAsync(AuditAcciones.Editar, AuditEntidades.AlertaStock, actual.Id, new { actual.IdProducto, actual.StockMinimo }, actual.IdBodega);
    }
    public async Task DeleteAsync(int id, int idBodega)
    {
        using var db = _factory.CreateDbContext();
        var alerta = await db.AlertaStock.FirstOrDefaultAsync(x => x.Id == id && x.IdBodega == idBodega) ?? throw new InvalidOperationException("La alerta no existe o no pertenece a la bodega activa.");
        db.AlertaStock.Remove(alerta); await db.SaveChangesAsync();
        await _audit.RegistrarActualAsync(AuditAcciones.Eliminar, AuditEntidades.AlertaStock, alerta.Id, new { alerta.IdProducto, alerta.StockMinimo }, alerta.IdBodega);
    }
    private static void Validar(AlertaStock alerta)
    {
        if (alerta.IdBodega <= 0 || alerta.IdProducto <= 0 || alerta.StockMinimo <= 0) throw new InvalidOperationException("Debe indicar un producto y un stock mínimo mayor que cero.");
    }
}
