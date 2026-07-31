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
    }
}
