using Microsoft.EntityFrameworkCore;
using System;

namespace BodegaDESAM.Services
{
    public class ModeloService
    {
        private readonly IDbContextFactory<PostgresDataContext> _factory;

        public ModeloService(IDbContextFactory<PostgresDataContext> db)
        {
            _factory = db;
        }

        /// <summary>
        /// Obtiene todos los modelos con su marca
        /// </summary>
        public async Task<List<Modelo>> GetAllAsync()
        {
            using var db = _factory.CreateDbContext();
            return await db.Modelo
                .Include(m => m.Marca)
                .OrderBy(m => m.Marca.Nombre)
                .ThenBy(m => m.Nombre)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene modelos filtrados por marca
        /// </summary>
        public async Task<List<Modelo>> GetByMarcaAsync(long idMarca)
        {
            using var db = _factory.CreateDbContext();
            return await db.Modelo
                .Where(m => m.id_marca == idMarca)
                .OrderBy(m => m.Nombre)
                .ToListAsync();
        }

        public async Task<Modelo?> GetByIdAsync(long id)
        {
            using var db = _factory.CreateDbContext();
            return await db.Modelo
                .Include(m => m.Marca)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task CreateAsync(Modelo modelo)
        {
            using var db = _factory.CreateDbContext();

            modelo.Nombre = (modelo.Nombre ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(modelo.Nombre))
                throw new InvalidOperationException("El nombre es obligatorio.");

            if (modelo.id_marca <= 0)
                throw new InvalidOperationException("Debe seleccionar una marca.");

            // Validar que no exista otro modelo con el mismo nombre en la misma marca
            var existe = await db.Modelo.AnyAsync(m => 
                m.id_marca == modelo.id_marca && 
                m.Nombre.ToLower() == modelo.Nombre.ToLower());

            if (existe)
                throw new InvalidOperationException("Ya existe un modelo con ese nombre para la marca seleccionada.");

            db.Modelo.Add(modelo);
            await db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Modelo modelo)
        {
            using var db = _factory.CreateDbContext();

            modelo.Nombre = (modelo.Nombre ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(modelo.Nombre))
                throw new InvalidOperationException("El nombre es obligatorio.");

            if (modelo.id_marca <= 0)
                throw new InvalidOperationException("Debe seleccionar una marca.");

            // Validar que no exista otro modelo con el mismo nombre en la misma marca (excluyendo el actual)
            var existe = await db.Modelo.AnyAsync(m => 
                m.Id != modelo.Id && 
                m.id_marca == modelo.id_marca && 
                m.Nombre.ToLower() == modelo.Nombre.ToLower());

            if (existe)
                throw new InvalidOperationException("Ya existe un modelo con ese nombre para la marca seleccionada.");

            db.Modelo.Update(modelo);
            await db.SaveChangesAsync();
        }
    }
}
