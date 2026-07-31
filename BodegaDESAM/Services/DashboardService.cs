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
        public List<UltimoMovimientoDto> UltimosMovimientos { get; set; } = new();
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

        public async Task<DashboardResumenDto> GetResumenAsync()
        {
            using var db = _factory.CreateDbContext();

            var inventario = await _inventarioService.GetInventarioActualAsync();

            var hoy = DateTime.Today;
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
            var inicioMesDate = DateOnly.FromDateTime(inicioMes);

            var entradasEsteMes = await db.Entrada
                .AsNoTracking()
                .CountAsync(e => e.Fecha >= inicioMesDate);

            var salidasEsteMes = await db.Salida
                .AsNoTracking()
                .CountAsync(s => s.Fecha >= inicioMesDate);

            var ajustesEsteMes = await db.AjusteInventario
                .AsNoTracking()
                .CountAsync(a => a.Fecha >= inicioMesDate);

            // Últimos 10 movimientos (entradas + salidas combinados)
            var ultimasEntradas = await db.Entrada
                .AsNoTracking()
                .Include(e => e.Proveedor)
                .OrderByDescending(e => e.Fecha)
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
                .OrderByDescending(s => s.Fecha)
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
                .OrderByDescending(a => a.Fecha)
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
                UltimosMovimientos = ultimos10
            };
        }
    }
}
