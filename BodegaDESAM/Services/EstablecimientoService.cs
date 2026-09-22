using Microsoft.EntityFrameworkCore;

namespace BodegaDESAM.Services
{
    public class EstablecimientoService
    {
        private readonly IDbContextFactory<PostgresDataContext> _factory;
        private readonly AuditService _audit;

        public EstablecimientoService(IDbContextFactory<PostgresDataContext> factory, AuditService audit)
        {
            _factory = factory;
            _audit = audit;
        }

    public async Task<List<Establecimiento>> GetAllAsync()
        {
            using var db = _factory.CreateDbContext();
            return await db.Establecimiento
                .AsNoTracking()
                .OrderBy(e => e.Nombre)
                .ToListAsync();
    }

    public async Task<PagedResult<Establecimiento>> GetPageAsync(PageRequest request, CancellationToken cancellationToken = default)
    {
        using var db = _factory.CreateDbContext();
        var query = db.Establecimiento.AsNoTracking().AsQueryable();
        int? id = int.TryParse(request.Search, out var parsedId) ? parsedId : null;
        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(e => EF.Functions.ILike(e.Nombre, $"%{request.Search}%") || (id.HasValue && e.Id == id.Value));
        return await query.OrderBy(e => e.Nombre).ThenBy(e => e.Id).ToPagedAsync(request, cancellationToken);
    }

        public async Task<Establecimiento?> GetByIdAsync(int id)
        {
            using var db = _factory.CreateDbContext();
            return await db.Establecimiento
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task CreateAsync(Establecimiento establecimiento)
        {
            using var db = _factory.CreateDbContext();
            establecimiento.Nombre = NormalizarNombre(establecimiento.Nombre);
            await ValidarNombreDisponibleAsync(db, establecimiento.Nombre, null);

            db.Establecimiento.Add(establecimiento);
            await db.SaveChangesAsync();
            await _audit.RegistrarActualAsync(AuditAcciones.Crear, "Establecimiento", establecimiento.Id, new { establecimiento.Nombre });
        }

        public async Task UpdateAsync(Establecimiento establecimiento)
        {
            using var db = _factory.CreateDbContext();
            establecimiento.Nombre = NormalizarNombre(establecimiento.Nombre);
            await ValidarNombreDisponibleAsync(db, establecimiento.Nombre, establecimiento.Id);

            db.Establecimiento.Update(establecimiento);
            await db.SaveChangesAsync();
            await _audit.RegistrarActualAsync(AuditAcciones.Editar, "Establecimiento", establecimiento.Id, new { establecimiento.Nombre });
        }

        public async Task DeleteAsync(int id)
        {
            using var db = _factory.CreateDbContext();
            var establecimiento = await db.Establecimiento.FirstOrDefaultAsync(e => e.Id == id);
            if (establecimiento is null)
                return;

            if (await db.Salida.AnyAsync(s => s.id_establecimiento == id))
                throw new InvalidOperationException("No se puede eliminar el establecimiento porque tiene salidas asociadas.");

            db.Establecimiento.Remove(establecimiento);
            await db.SaveChangesAsync();
            await _audit.RegistrarActualAsync(AuditAcciones.Eliminar, "Establecimiento", id, new { establecimiento.Nombre });
        }

        private static string NormalizarNombre(string? nombre) => nombre?.Trim() ?? string.Empty;

        private static async Task ValidarNombreDisponibleAsync(
            PostgresDataContext db,
            string nombre,
            int? idActual)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new InvalidOperationException("El nombre del establecimiento es obligatorio.");

            var nombreNormalizado = nombre.ToUpper();
            var existe = await db.Establecimiento.AnyAsync(e =>
                e.Nombre.ToUpper() == nombreNormalizado &&
                (!idActual.HasValue || e.Id != idActual.Value));

            if (existe)
                throw new InvalidOperationException("Ya existe un establecimiento con ese nombre.");
        }
    }
}
