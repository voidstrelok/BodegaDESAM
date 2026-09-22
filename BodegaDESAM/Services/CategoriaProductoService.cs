using Microsoft.EntityFrameworkCore;

namespace BodegaDESAM.Services
{
    public class CategoriaProductoService
    {
        private readonly IDbContextFactory<PostgresDataContext> _factory;

        public CategoriaProductoService(IDbContextFactory<PostgresDataContext> db)
        {
            _factory = db;
        }

        public async Task<List<CategoriaProducto>> GetAllAsync()
        {
            using var db = _factory.CreateDbContext();
            return await db.CategoriaProducto
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        public async Task<PagedResult<CategoriaProducto>> GetPageAsync(PageRequest request, CancellationToken cancellationToken = default)
        {
            using var db = _factory.CreateDbContext();
            var query = db.CategoriaProducto.AsNoTracking().AsQueryable();
            long? id = long.TryParse(request.Search, out var parsedId) ? parsedId : null;
            if (!string.IsNullOrWhiteSpace(request.Search))
                query = query.Where(c => EF.Functions.ILike(c.Nombre, $"%{request.Search}%") || (id.HasValue && c.Id == id.Value));
            return await query.OrderBy(c => c.Nombre).ThenBy(c => c.Id).ToPagedAsync(request, cancellationToken);
        }

        public async Task<CategoriaProducto?> GetByIdAsync(long id)
        {
            using var db = _factory.CreateDbContext();
            return await db.CategoriaProducto.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task CreateAsync(CategoriaProducto categoria)
        {
            using var db = _factory.CreateDbContext();
            db.CategoriaProducto.Add(categoria);
            await db.SaveChangesAsync();
        }

        public async Task UpdateAsync(CategoriaProducto categoria)
        {
            using var db = _factory.CreateDbContext();
            db.CategoriaProducto.Update(categoria);
            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(long id)
        {
            using var db = _factory.CreateDbContext();

            var tieneProductos = await db.Producto.AnyAsync(p => p.id_categoria_producto == id);
            if (tieneProductos)
                throw new InvalidOperationException("La categoría tiene productos asociados.");

            var categoria = await db.CategoriaProducto.FirstOrDefaultAsync(c => c.Id == id);
            if (categoria == null)
                return;

            db.CategoriaProducto.Remove(categoria);
            await db.SaveChangesAsync();
        }
    }
}
