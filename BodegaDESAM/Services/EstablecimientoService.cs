using Microsoft.EntityFrameworkCore;

namespace BodegaDESAM.Services
{
    public class EstablecimientoService
    {
        private readonly IDbContextFactory<PostgresDataContext> _factory;

        public EstablecimientoService(IDbContextFactory<PostgresDataContext> factory)
        {
            _factory = factory;
        }

        public async Task<List<Establecimiento>> GetAllAsync()
        {
            using var db = _factory.CreateDbContext();
            return await db.Establecimiento
                .AsNoTracking()
                .OrderBy(e => e.Nombre)
                .ToListAsync();
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
        }

        public async Task UpdateAsync(Establecimiento establecimiento)
        {
            using var db = _factory.CreateDbContext();
            establecimiento.Nombre = NormalizarNombre(establecimiento.Nombre);
            await ValidarNombreDisponibleAsync(db, establecimiento.Nombre, establecimiento.Id);

            db.Establecimiento.Update(establecimiento);
            await db.SaveChangesAsync();
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
