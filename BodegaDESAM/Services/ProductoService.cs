using Microsoft.EntityFrameworkCore;
using System;

namespace BodegaDESAM.Services
{
    public class ProductoService
    {
        private readonly IDbContextFactory<PostgresDataContext> _factory;
        private readonly AuditService _audit;

        public ProductoService(IDbContextFactory<PostgresDataContext> db, AuditService audit)
        {
            _factory = db;
            _audit = audit;
        }

        public async Task<List<Producto>> GetAllAsync()
        {
            using var db = _factory.CreateDbContext();
            return await db.Producto
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<PagedResult<Producto>> GetPageAsync(PageRequest request, CancellationToken cancellationToken = default)
        {
            using var db = _factory.CreateDbContext();
            var query = db.Producto.AsNoTracking().AsQueryable();
            int? id = int.TryParse(request.Search, out var parsedId) ? parsedId : null;
            if (!string.IsNullOrWhiteSpace(request.Search))
                query = query.Where(p => EF.Functions.ILike(p.Nombre, $"%{request.Search}%") || (id.HasValue && p.Id == id.Value));
            return await query.OrderBy(p => p.Nombre).ThenBy(p => p.Id).ToPagedAsync(request, cancellationToken);
        }

        public async Task<Producto?> GetByIdAsync(int id)
        {
            using var db = _factory.CreateDbContext();
            return await db.Producto.FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task CreateAsync(Producto producto)
        {
            using var db = _factory.CreateDbContext();
            db.Producto.Add(producto);
            await db.SaveChangesAsync();
            await _audit.RegistrarActualAsync(AuditAcciones.Crear, "Producto", producto.Id, new { producto.Nombre, producto.id_categoria_producto });
        }


        public async Task UpdateAsync(Producto producto)
        {
            using var db = _factory.CreateDbContext();
            db.Producto.Update(producto);
            await db.SaveChangesAsync();
            await _audit.RegistrarActualAsync(AuditAcciones.Editar, "Producto", producto.Id, new { producto.Nombre, producto.id_categoria_producto });
        }
    }
}
