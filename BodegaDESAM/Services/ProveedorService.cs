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
