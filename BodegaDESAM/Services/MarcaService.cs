using Microsoft.EntityFrameworkCore;
using System;

namespace BodegaDESAM.Services
{
    public class MarcaService
    {
        private readonly IDbContextFactory<PostgresDataContext> _factory;

        public MarcaService(IDbContextFactory<PostgresDataContext> db)
        {
            _factory = db;
        }

        public async Task<List<Marca>> GetAllAsync()
        {
            using var db = _factory.CreateDbContext();
            return await db.Marca
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }
        public async Task<Marca?> GetByIdAsync(int id)
        {
            using var db = _factory.CreateDbContext();
            return await db.Marca.FirstOrDefaultAsync(m => m.Id == id);
        }
        public async Task CreateAsync(Marca marca)
        {
            using var db = _factory.CreateDbContext();

            marca.Nombre = (marca.Nombre ?? string.Empty).Trim();
            var existe = await db.Marca.AnyAsync(m => m.Nombre.ToLower() == marca.Nombre.ToLower());
            if (existe)
                throw new InvalidOperationException("Ya existe una marca con ese nombre.");

            db.Marca.Add(marca);
            await db.SaveChangesAsync();
        }


        public async Task UpdateAsync(Marca marca)
        {
            using var db = _factory.CreateDbContext();

            marca.Nombre = (marca.Nombre ?? string.Empty).Trim();
            var existe = await db.Marca.AnyAsync(m => m.Id != marca.Id && m.Nombre.ToLower() == marca.Nombre.ToLower());
            if (existe)
                throw new InvalidOperationException("Ya existe una marca con ese nombre.");

            db.Marca.Update(marca);
            await db.SaveChangesAsync();
        }
    }
}
