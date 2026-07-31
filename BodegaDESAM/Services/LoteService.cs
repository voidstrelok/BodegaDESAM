using Microsoft.EntityFrameworkCore;

namespace BodegaDESAM.Services
{
    public class LoteService
    {
        private readonly IDbContextFactory<PostgresDataContext> _factory;

        public LoteService(IDbContextFactory<PostgresDataContext> factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Busca un lote por producto+código, o lo crea si no existe.
        /// </summary>
        public async Task<Lote> GetOrCreateAsync(int idProducto, string codigo)
        {
            using var db = _factory.CreateDbContext();
            var lote = await db.Lote
                .FirstOrDefaultAsync(l => l.id_producto == idProducto && l.Codigo == codigo);

            if (lote is null)
            {
                lote = new Lote { id_producto = idProducto, Codigo = codigo };
                db.Lote.Add(lote);
                await db.SaveChangesAsync();
            }

            return lote;
        }
    }
}
