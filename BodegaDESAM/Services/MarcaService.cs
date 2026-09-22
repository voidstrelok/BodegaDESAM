using Microsoft.EntityFrameworkCore;
using System;

namespace BodegaDESAM.Services
{
    public class MarcaService
    {
        private readonly IDbContextFactory<PostgresDataContext> _factory;
        private readonly AuditService _audit;

        public MarcaService(IDbContextFactory<PostgresDataContext> db, AuditService audit)
        {
            _factory = db;
            _audit = audit;
        }

        public async Task<List<Marca>> GetAllAsync()
        {
            using var db = _factory.CreateDbContext();
            return await db.Marca
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<PagedResult<Marca>> GetPageAsync(PageRequest request, CancellationToken cancellationToken = default)
        {
            using var db = _factory.CreateDbContext();
            var query = db.Marca.AsNoTracking().AsQueryable();
            long? id = long.TryParse(request.Search, out var parsedId) ? parsedId : null;
            if (!string.IsNullOrWhiteSpace(request.Search))
                query = query.Where(m => EF.Functions.ILike(m.Nombre, $"%{request.Search}%") || (id.HasValue && m.Id == id.Value));
            return await query.OrderBy(m => m.Nombre).ThenBy(m => m.Id).ToPagedAsync(request, cancellationToken);
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
            await _audit.RegistrarActualAsync(AuditAcciones.Crear, "Marca", (int)marca.Id, new { marca.Nombre });
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
            await _audit.RegistrarActualAsync(AuditAcciones.Editar, "Marca", (int)marca.Id, new { marca.Nombre });
        }
    }
}
