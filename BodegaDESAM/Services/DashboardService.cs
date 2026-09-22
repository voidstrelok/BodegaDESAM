using Microsoft.EntityFrameworkCore;

namespace BodegaDESAM.Services
{
    public class DashboardResumenDto
    {
        public int TotalProductosDistintos { get; set; }
        public long TotalUnidadesEnStock { get; set; }
        public int EntradasEsteMes { get; set; }
        public int SalidasEsteMes { get; set; }
        public int AjustesEsteMes { get; set; }
        public decimal ValorInventario { get; set; }
        public decimal ValorEntradasEsteMes { get; set; }
        public decimal ValorSalidasEsteMes { get; set; }
        public List<DashboardProductoDto> ProductosMasEntraron { get; set; } = new();
        public List<DashboardProductoDto> ProductosMasSalieron { get; set; } = new();
        public List<SalidaPorEstablecimientoDto> SalidasPorEstablecimiento { get; set; } = new();
        public List<UltimoMovimientoDto> UltimosMovimientos { get; set; } = new();
    }

    public class DashboardProductoDto
    {
        public string Nombre { get; set; } = string.Empty;
        public long Unidades { get; set; }
        public decimal Valor { get; set; }
    }

    public class SalidaPorEstablecimientoDto
    {
        public string Nombre { get; set; } = string.Empty;
        public long Unidades { get; set; }
        public decimal Valor { get; set; }
        public int Documentos { get; set; }
    }

    public class UltimoMovimientoDto
    {
        public string Tipo { get; set; } = string.Empty;
        public int IdDocumento { get; set; }
        public DateOnly Fecha { get; set; }
        public string? Referencia { get; set; }
        public string? Contraparte { get; set; }
    }

    public class DashboardService
    {
        private readonly IDbContextFactory<PostgresDataContext> _factory;
        private readonly InventarioService _inventarioService;

        public DashboardService(
            IDbContextFactory<PostgresDataContext> factory,
            InventarioService inventarioService)
        {
            _factory = factory;
            _inventarioService = inventarioService;
        }

        public async Task<DashboardResumenDto> GetResumenAsync(int idBodega)
        {
            using var db = _factory.CreateDbContext();

            var inventario = await _inventarioService.GetInventarioActualAsync(idBodega);

            var hoy = DateTime.Today;
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
            var inicioMesDate = DateOnly.FromDateTime(inicioMes);

            var entradasEsteMes = await db.Entrada
                .AsNoTracking()
                .CountAsync(e => e.id_bodega == idBodega && e.Fecha >= inicioMesDate);

            var salidasEsteMes = await db.Salida
                .AsNoTracking()
                .CountAsync(s => s.id_bodega == idBodega && s.Fecha >= inicioMesDate);

            var ajustesEsteMes = await db.AjusteInventario
                .AsNoTracking()
                .CountAsync(a => a.id_bodega == idBodega && a.Fecha >= inicioMesDate);

            var entradasMes = await db.DetalleEntrada
                .AsNoTracking()
                .Where(d => d.Entrada.id_bodega == idBodega && d.Entrada.Fecha >= inicioMesDate)
                .Select(d => new { Nombre = d.Producto.Nombre, d.Cantidad, d.ValorUnitario })
                .ToListAsync();

            var salidasMes = await db.DetalleSalida
                .AsNoTracking()
                .Where(d => d.Salida.id_bodega == idBodega && d.Salida.Fecha >= inicioMesDate)
                .Select(d => new
                {
                    Producto = d.Producto.Nombre,
                    Establecimiento = d.Salida.EStablecimiento.Nombre,
                    d.id_salida,
                    d.Cantidad,
                    ValorEntrada = d.DetalleEntrada == null ? null : d.DetalleEntrada.ValorUnitario,
                    ValorAjuste = d.DetalleAjusteOrigen == null ? null : d.DetalleAjusteOrigen.ValorUnitario
                })
                .ToListAsync();

            var productosMasEntraron = entradasMes
                .GroupBy(d => d.Nombre)
                .Select(g => new DashboardProductoDto
                {
                    Nombre = g.Key,
                    Unidades = g.Sum(x => x.Cantidad),
                    Valor = g.Sum(x => (decimal)x.Cantidad * (x.ValorUnitario ?? 0))
                })
                .OrderByDescending(x => x.Unidades).ThenBy(x => x.Nombre).Take(5).ToList();

            var productosMasSalieron = salidasMes
                .GroupBy(d => d.Producto)
                .Select(g => new DashboardProductoDto
                {
                    Nombre = g.Key,
                    Unidades = g.Sum(x => (long)x.Cantidad),
                    Valor = g.Sum(x => (decimal)x.Cantidad * (x.ValorEntrada ?? x.ValorAjuste ?? 0))
                })
                .OrderByDescending(x => x.Unidades).ThenBy(x => x.Nombre).Take(5).ToList();

            var salidasPorEstablecimiento = salidasMes
                .GroupBy(d => d.Establecimiento)
                .Select(g => new SalidaPorEstablecimientoDto
                {
                    Nombre = g.Key,
                    Unidades = g.Sum(x => (long)x.Cantidad),
                    Valor = g.Sum(x => (decimal)x.Cantidad * (x.ValorEntrada ?? x.ValorAjuste ?? 0)),
                    Documentos = g.Select(x => x.id_salida).Distinct().Count()
                })
                .OrderByDescending(x => x.Unidades).ThenBy(x => x.Nombre).ToList();

            var entradasInventario = await db.DetalleEntrada.AsNoTracking()
                .Where(d => d.Entrada.id_bodega == idBodega)
                .Select(d => new { d.Id, d.Cantidad, d.ValorUnitario }).ToListAsync();
            var idsEntrada = entradasInventario.Select(x => x.Id).ToList();
            var salidasPorEntrada = idsEntrada.Count == 0 ? new Dictionary<int, long>() : await db.DetalleSalida.AsNoTracking()
                .Where(d => d.id_detalle_entrada.HasValue && idsEntrada.Contains(d.id_detalle_entrada.Value))
                .GroupBy(d => d.id_detalle_entrada!.Value).Select(g => new { Id = g.Key, Cantidad = g.Sum(x => (long)x.Cantidad) })
                .ToDictionaryAsync(x => x.Id, x => x.Cantidad);
            var ajustesPorEntrada = idsEntrada.Count == 0 ? new Dictionary<int, long>() : await db.DetalleAjuste.AsNoTracking()
                .Where(d => d.id_detalle_entrada_origen.HasValue && idsEntrada.Contains(d.id_detalle_entrada_origen.Value) && d.TipoAjuste == TipoAjuste.Disminucion)
                .GroupBy(d => d.id_detalle_entrada_origen!.Value).Select(g => new { Id = g.Key, Cantidad = g.Sum(x => x.Cantidad) })
                .ToDictionaryAsync(x => x.Id, x => x.Cantidad);

            var ajustesAumento = await db.DetalleAjuste.AsNoTracking()
                .Where(d => d.Ajuste.id_bodega == idBodega && d.TipoAjuste == TipoAjuste.Aumento)
                .Select(d => new { d.Id, d.Cantidad, d.ValorUnitario }).ToListAsync();
            var idsAjuste = ajustesAumento.Select(x => x.Id).ToList();
            var salidasPorAjuste = idsAjuste.Count == 0 ? new Dictionary<int, long>() : await db.DetalleSalida.AsNoTracking()
                .Where(d => d.id_detalle_ajuste_origen.HasValue && idsAjuste.Contains(d.id_detalle_ajuste_origen.Value))
                .GroupBy(d => d.id_detalle_ajuste_origen!.Value).Select(g => new { Id = g.Key, Cantidad = g.Sum(x => (long)x.Cantidad) })
                .ToDictionaryAsync(x => x.Id, x => x.Cantidad);
            var ajustesPorAjuste = idsAjuste.Count == 0 ? new Dictionary<int, long>() : await db.DetalleAjuste.AsNoTracking()
                .Where(d => d.id_detalle_ajuste_origen.HasValue && idsAjuste.Contains(d.id_detalle_ajuste_origen.Value) && d.TipoAjuste == TipoAjuste.Disminucion)
                .GroupBy(d => d.id_detalle_ajuste_origen!.Value).Select(g => new { Id = g.Key, Cantidad = g.Sum(x => x.Cantidad) })
                .ToDictionaryAsync(x => x.Id, x => x.Cantidad);

            var valorInventario = entradasInventario.Sum(x => (decimal)Math.Max(0, x.Cantidad - (salidasPorEntrada.GetValueOrDefault(x.Id) + ajustesPorEntrada.GetValueOrDefault(x.Id))) * (x.ValorUnitario ?? 0))
                + ajustesAumento.Sum(x => (decimal)Math.Max(0, x.Cantidad - (salidasPorAjuste.GetValueOrDefault(x.Id) + ajustesPorAjuste.GetValueOrDefault(x.Id))) * (x.ValorUnitario ?? 0));

            // Últimos 10 movimientos (entradas + salidas combinados)
            var ultimasEntradas = await db.Entrada
                .AsNoTracking()
                .Include(e => e.Proveedor)
                .Where(e => e.id_bodega == idBodega).OrderByDescending(e => e.Fecha)
                .ThenByDescending(e => e.Id)
                .Take(10)
                .Select(e => new UltimoMovimientoDto
                {
                    Tipo = "Entrada",
                    IdDocumento = e.Id,
                    Fecha = e.Fecha,
                    Referencia = e.NDocumento,
                    Contraparte = e.Proveedor != null ? e.Proveedor.Nombre : null
                })
                .ToListAsync();

            var ultimasSalidas = await db.Salida
                .AsNoTracking()
                .Include(s => s.EStablecimiento)
                .Where(s => s.id_bodega == idBodega).OrderByDescending(s => s.Fecha)
                .ThenByDescending(s => s.Id)
                .Take(10)
                .Select(s => new UltimoMovimientoDto
                {
                    Tipo = "Salida",
                    IdDocumento = s.Id,
                    Fecha = s.Fecha,
                    Referencia = s.Solicitante,
                    Contraparte = s.EStablecimiento != null ? s.EStablecimiento.Nombre : null
                })
                .ToListAsync();

            var ultimosAjustes = await db.AjusteInventario
                .AsNoTracking()
                .Include(a => a.Bodega)
                .Where(a => a.id_bodega == idBodega).OrderByDescending(a => a.Fecha)
                .ThenByDescending(a => a.Id)
                .Take(10)
                .Select(a => new UltimoMovimientoDto
                {
                    Tipo = "Ajuste",
                    IdDocumento = a.Id,
                    Fecha = a.Fecha,
                    Referencia = a.Motivo,
                    Contraparte = a.Bodega != null ? a.Bodega.Nombre : null
                })
                .ToListAsync();

            var ultimos10 = ultimasEntradas
                .Concat(ultimasSalidas)
                .Concat(ultimosAjustes)
                .OrderByDescending(m => m.Fecha)
                .ThenByDescending(m => m.IdDocumento)
                .Take(10)
                .ToList();

            return new DashboardResumenDto
            {
                TotalProductosDistintos = inventario.Select(i => i.IdProducto).Distinct().Count(),
                TotalUnidadesEnStock = inventario.Where(i => i.StockActual > 0).Sum(i => i.StockActual),
                EntradasEsteMes = entradasEsteMes,
                SalidasEsteMes = salidasEsteMes,
                AjustesEsteMes = ajustesEsteMes,
                ValorInventario = valorInventario,
                ValorEntradasEsteMes = entradasMes.Sum(x => (decimal)x.Cantidad * (x.ValorUnitario ?? 0)),
                ValorSalidasEsteMes = salidasMes.Sum(x => (decimal)x.Cantidad * (x.ValorEntrada ?? x.ValorAjuste ?? 0)),
                ProductosMasEntraron = productosMasEntraron,
                ProductosMasSalieron = productosMasSalieron,
                SalidasPorEstablecimiento = salidasPorEstablecimiento,
                UltimosMovimientos = ultimos10
            };
        }
    }
}
