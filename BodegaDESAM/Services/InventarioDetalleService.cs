using Microsoft.EntityFrameworkCore;

namespace BodegaDESAM.Services
{
    public sealed record MovimientoProductoDetalleDto(
        string Tipo,
        int IdDocumento,
        DateTime Fecha,
        string? Referencia,
        string? Ubicacion,
        string? Detalle,
        long Cantidad,
        List<string> Series);

    public class InventarioDetalleService
    {
        private readonly IDbContextFactory<PostgresDataContext> _factory;

        public InventarioDetalleService(IDbContextFactory<PostgresDataContext> factory)
        {
            _factory = factory;
        }

        public async Task<List<MovimientoProductoDetalleDto>> GetMovimientosDetalleAsync(
            int idProducto,
            long? idMarca,
            long? idModelo,
            string? serieFiltro,
            int? idEstablecimiento,
            int? idBodega = null)
        {
            using var db = _factory.CreateDbContext();

            var entradasBase = await db.DetalleEntrada
                .AsNoTracking()
                .Where(d => d.id_producto == idProducto
                    && (!idBodega.HasValue || d.Entrada.id_bodega == idBodega.Value)
                    && (!idMarca.HasValue || d.id_marca == idMarca.Value)
                    && d.id_modelo == idModelo)
                .Select(d => new
                {
                    DetalleEntradaId = d.Id,
                    IdEntrada = d.id_entrada,
                    Fecha = d.Entrada.Fecha.ToDateTime(TimeOnly.MinValue),
                    Referencia = d.Entrada.NDocumento,
                    Bodega = d.Entrada.Bodega.Nombre,
                    Detalle = (string?)null,
                    Cantidad = (long)d.Cantidad,
                })
                .ToListAsync();

            var detalleEntradaIds = entradasBase.Select(x => x.DetalleEntradaId).ToList();

            var seriesEntrada = await db.ProductoSerie
                .AsNoTracking()
                .Where(s => s.id_producto == idProducto && s.id_detalle_entrada.HasValue && detalleEntradaIds.Contains(s.id_detalle_entrada.Value))
                .Select(s => new { s.id_detalle_entrada, s.Serie })
                .ToListAsync();

            var seriesPorDetalleEntrada = seriesEntrada
                .GroupBy(x => x.id_detalle_entrada!.Value)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Serie).OrderBy(x => x).ToList());

            var salidasBaseQ = db.DetalleSalida
                .AsNoTracking()
                .Where(d => d.id_producto == idProducto
                    && (!idBodega.HasValue || d.Salida.id_bodega == idBodega.Value)
                    && (!idMarca.HasValue || d.id_marca == idMarca.Value)
                    && d.id_modelo == idModelo)
                .Select(d => new
                {
                    IdSalida = d.id_salida,
                    Fecha = d.Salida.Fecha.ToDateTime(TimeOnly.MinValue),
                    Referencia = d.Salida.Solicitante,
                    Establecimiento = d.Salida.EStablecimiento.Nombre,
                    IdEstablecimiento = d.Salida.id_establecimiento,
                    Cantidad = (long)d.Cantidad,
                    Detalle = (string?)null,
                });

            if (idEstablecimiento != null)
                salidasBaseQ = salidasBaseQ.Where(x => x.IdEstablecimiento == idEstablecimiento);

            var salidasBase = await salidasBaseQ.ToListAsync();

            var salidaIds = salidasBase.Select(x => x.IdSalida).Distinct().ToList();

            var seriesSalidaQ = db.ProductoSerie
                .AsNoTracking()
                .Where(s => s.id_producto == idProducto && s.id_salida != null && salidaIds.Contains(s.id_salida.Value));

            if (!string.IsNullOrWhiteSpace(serieFiltro))
                seriesSalidaQ = seriesSalidaQ.Where(s => EF.Functions.ILike(s.Serie, $"%{serieFiltro}%"));

            var seriesSalida = await seriesSalidaQ
                .Select(s => new { IdSalida = s.id_salida!.Value, s.Serie })
                .ToListAsync();

            var seriesPorSalida = seriesSalida
                .GroupBy(x => x.IdSalida)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Serie).OrderBy(x => x).ToList());

            var result = new List<MovimientoProductoDetalleDto>();

            foreach (var e in entradasBase)
            {
                var series = seriesPorDetalleEntrada.TryGetValue(e.DetalleEntradaId, out var se)
                    ? se
                    : new List<string>();

                if (!string.IsNullOrWhiteSpace(serieFiltro))
                {
                    series = series.Where(s => s.Contains(serieFiltro, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (series.Count == 0) continue;
                }

                result.Add(new MovimientoProductoDetalleDto(
                    "Entrada",
                    e.IdEntrada,
                    e.Fecha,
                    e.Referencia,
                    e.Bodega,
                    e.Detalle,
                    e.Cantidad,
                    series));
            }

            foreach (var s in salidasBase)
            {
                var series = seriesPorSalida.TryGetValue(s.IdSalida, out var ss)
                    ? ss
                    : new List<string>();

                if (!string.IsNullOrWhiteSpace(serieFiltro) && series.Count == 0)
                    continue;

                result.Add(new MovimientoProductoDetalleDto(
                    "Salida",
                    s.IdSalida,
                    s.Fecha,
                    s.Referencia,
                    s.Establecimiento,
                    s.Detalle,
                    -1 * s.Cantidad,
                    series));
            }

            var ajustesBase = await db.DetalleAjuste
                .AsNoTracking()
                .Where(d => d.id_producto == idProducto
                    && (!idBodega.HasValue || d.Ajuste.id_bodega == idBodega.Value)
                    && (!idMarca.HasValue || d.id_marca == idMarca.Value)
                    && d.id_modelo == idModelo)
                .Select(d => new
                {
                    DetalleAjusteId = d.Id,
                    IdAjuste = d.id_ajuste,
                    Fecha = d.Ajuste.Fecha.ToDateTime(TimeOnly.MinValue),
                    Motivo = d.Ajuste.Motivo,
                    Bodega = d.Ajuste.Bodega.Nombre,
                    Observacion = d.Observacion,
                    Tipo = d.TipoAjuste,
                    Cantidad = (long)d.Cantidad
                })
                .ToListAsync();

            var detalleAjusteIds = ajustesBase.Select(x => x.DetalleAjusteId).ToList();
            var seriesAjuste = await db.ProductoSerie.AsNoTracking()
                .Where(s => s.id_producto == idProducto && s.id_detalle_ajuste.HasValue
                    && detalleAjusteIds.Contains(s.id_detalle_ajuste.Value))
                .Select(s => new { IdDetalleAjuste = s.id_detalle_ajuste!.Value, s.Serie })
                .ToListAsync();

            var seriesPorDetalleAjuste = seriesAjuste
                .GroupBy(x => x.IdDetalleAjuste)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Serie).OrderBy(x => x).ToList());

            foreach (var a in ajustesBase)
            {
                var series = seriesPorDetalleAjuste.TryGetValue(a.DetalleAjusteId, out var sa)
                    ? sa
                    : new List<string>();

                if (!string.IsNullOrWhiteSpace(serieFiltro))
                {
                    series = series.Where(s => s.Contains(serieFiltro, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (series.Count == 0) continue;
                }

                var tipo = a.Tipo == TipoAjuste.Aumento ? "Ajuste (+)" : "Ajuste (-)";
                var detalle = string.Join(" · ", new[] { a.Motivo, a.Observacion }
                    .Where(x => !string.IsNullOrWhiteSpace(x)));
                result.Add(new MovimientoProductoDetalleDto(
                    tipo,
                    a.IdAjuste,
                    a.Fecha,
                    $"Ajuste #{a.IdAjuste}",
                    a.Bodega,
                    detalle,
                    a.Tipo == TipoAjuste.Aumento ? a.Cantidad : -a.Cantidad,
                    series));
            }

            return result
                .OrderByDescending(x => x.Fecha)
                .ThenByDescending(x => x.IdDocumento)
                .ToList();
        }
    }
}
