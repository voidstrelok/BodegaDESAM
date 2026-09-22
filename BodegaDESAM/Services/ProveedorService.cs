using Microsoft.EntityFrameworkCore;
using System;

namespace BodegaDESAM.Services
{
    public class ProveedorService
    {
        private readonly IDbContextFactory<PostgresDataContext> _factory;

        public ProveedorService(IDbContextFactory<PostgresDataContext> db)
        {
            _factory = db;
        }

        public async Task<List<Proveedor>> GetAllAsync()
        {
            using var db = _factory.CreateDbContext();
            return await db.Proveedor
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<PagedResult<Proveedor>> GetPageAsync(PageRequest request, CancellationToken cancellationToken = default)
        {
            using var db = _factory.CreateDbContext();
            var query = db.Proveedor.AsNoTracking().AsQueryable();
            long? id = long.TryParse(request.Search, out var parsedId) ? parsedId : null;
            if (!string.IsNullOrWhiteSpace(request.Search))
                query = query.Where(p =>
                    EF.Functions.ILike(p.Nombre, $"%{request.Search}%") ||
                    EF.Functions.ILike(p.RUT ?? string.Empty, $"%{request.Search}%") ||
                    EF.Functions.ILike(p.Telefono ?? string.Empty, $"%{request.Search}%") ||
                    EF.Functions.ILike(p.Email ?? string.Empty, $"%{request.Search}%") ||
                    EF.Functions.ILike(p.Direccion ?? string.Empty, $"%{request.Search}%") ||
                    (id.HasValue && p.Id == id.Value));
            return await query.OrderBy(p => p.Nombre).ThenBy(p => p.Id).ToPagedAsync(request, cancellationToken);
        }
        public async Task<Proveedor?> GetByIdAsync(long id)
        {
            using var db = _factory.CreateDbContext();
            return await db.Proveedor.FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task CreateAsync(Proveedor proveedor)
        {
            using var db = _factory.CreateDbContext();
            db.Proveedor.Add(proveedor);
            await db.SaveChangesAsync();
        }


        public async Task UpdateAsync(Proveedor proveedor)
        {
            using var db = _factory.CreateDbContext();
            db.Proveedor.Update(proveedor);
            await db.SaveChangesAsync();
        }
    }
}
