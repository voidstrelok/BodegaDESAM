using Microsoft.EntityFrameworkCore;

namespace BodegaDESAM.Services
{
    public sealed record MovimientoProductoDto(
        string Tipo,
        int IdDocumento,
        DateTime Fecha,
        string? Referencia,
        long Cantidad);

    public class MovimientoProductoService
    {
        private readonly IDbContextFactory<PostgresDataContext> _factory;

        public MovimientoProductoService(IDbContextFactory<PostgresDataContext> factory)
        {
            _factory = factory;
        }

        public async Task<List<MovimientoProductoDto>> GetMovimientosAsync(int idProducto)
        {
            using var db = _factory.CreateDbContext();

            var entradas = await db.DetalleEntrada
                .AsNoTracking()
                .Where(d => d.id_producto == idProducto)
                .Select(d => new MovimientoProductoDto(
                    "Entrada",
                    d.id_entrada,
                    d.Entrada.Fecha.ToDateTime(TimeOnly.MinValue),
                    d.Entrada.NDocumento,
                    (long)d.Cantidad))
                .ToListAsync();

            var salidas = await db.DetalleSalida
                .AsNoTracking()
                .Where(d => d.id_producto == idProducto)
                .Select(d => new MovimientoProductoDto(
                    "Salida",
                    d.id_salida,
                    d.Salida.Fecha.ToDateTime(TimeOnly.MinValue),
                    d.Salida.Solicitante,
                    (long)d.Cantidad * -1))
                .ToListAsync();

            return entradas
                .Concat(salidas)
                .OrderByDescending(x => x.Fecha)
                .ThenByDescending(x => x.IdDocumento)
                .ToList();
        }
    }
}
