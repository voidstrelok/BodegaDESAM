using Microsoft.EntityFrameworkCore;

namespace BodegaDESAM.Services
{
    public enum SeveridadAlerta
    {
        Info,
        Advertencia,
        Critica
    }

    public class AlertaDto
    {
        public string TipoAlerta { get; set; } = string.Empty;
        public SeveridadAlerta Severidad { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public int IdReferencia { get; set; }
        public string? TipoMovimiento { get; set; }
        public int? IdMovimiento { get; set; }
    }

    public class AlertaService
    {
        private readonly IDbContextFactory<PostgresDataContext> _factory;
        private readonly InventarioService _inventarioService;

        private const int DiasProximoVencimiento = 30;

        // Permite avisar a los componentes que muestran el contador de alertas
        // cuando un movimiento cambia el stock o los vencimientos.
        public event Func<Task>? AlertasActualizadas;

        public AlertaService(
            IDbContextFactory<PostgresDataContext> factory,
            InventarioService inventarioService)
        {
            _factory = factory;
            _inventarioService = inventarioService;
        }

        public async Task<List<AlertaDto>> GetAlertasActivasAsync(int? idBodega = null)
        {
            var alertas = new List<AlertaDto>();

            alertas.AddRange(await GetAlertasStockBajoAsync(idBodega));
            alertas.AddRange(await GetAlertasLotesPorVencerAsync(idBodega));

            return alertas;
        }

        public async Task NotificarActualizacionAsync()
        {
            var handlers = AlertasActualizadas?.GetInvocationList();
            if (handlers is null)
                return;

            foreach (var handler in handlers.Cast<Func<Task>>())
                await handler();
        }

        /// <summary>
        /// Productos cuyo stock actual es igual o inferior al umbral configurado para la bodega.
        /// </summary>
        public async Task<List<AlertaDto>> GetAlertasStockBajoAsync(int? idBodega = null)
        {
            using var db = _factory.CreateDbContext();

            if (!idBodega.HasValue || idBodega.Value <= 0)
                return new List<AlertaDto>();

            var configuraciones = await db.AlertaStock
                .AsNoTracking()
                .Include(a => a.Producto)
                .Where(a => a.IdBodega == idBodega.Value)
                .Select(a => new { a.Id, a.IdProducto, Nombre = a.Producto.Nombre, a.StockMinimo })
                .ToListAsync();

            if (configuraciones.Count == 0)
                return new List<AlertaDto>();

            var inventario = await _inventarioService.GetInventarioActualAsync(idBodega);

            var alertas = new List<AlertaDto>();

            foreach (var configuracion in configuraciones)
            {
                var stockActual = inventario
                    .Where(i => i.IdProducto == configuracion.IdProducto)
                    .Sum(i => i.StockActual);

                if (stockActual <= configuracion.StockMinimo)
                {
                    var severidad = stockActual == 0
                        ? SeveridadAlerta.Critica
                        : SeveridadAlerta.Advertencia;

                    alertas.Add(new AlertaDto
                    {
                        TipoAlerta = "StockBajo",
                        Severidad = severidad,
                        Mensaje = stockActual == 0
                            ? $"Sin stock: '{configuracion.Nombre}' (mínimo: {configuracion.StockMinimo})"
                            : $"Stock bajo: '{configuracion.Nombre}' — actual: {stockActual}, mínimo: {configuracion.StockMinimo}",
                        IdReferencia = configuracion.IdProducto
                    });
                }
            }

            return alertas;
        }

        /// <summary>
        /// Lotes con FechaVencimiento dentro de los próximos 30 días (o ya vencidos),
        /// y DetalleEntrada con FechaVencimiento directa (sin lote o lote sin fecha).
        /// </summary>
        public async Task<List<AlertaDto>> GetAlertasLotesPorVencerAsync(int? idBodega = null)
        {
            using var db = _factory.CreateDbContext();

            var hoy = DateOnly.FromDateTime(DateTime.Today);
            var limite = DateOnly.FromDateTime(DateTime.Today.AddDays(DiasProximoVencimiento));

            var alertas = new List<AlertaDto>();

            // 1) Alertas desde Lote.FechaVencimiento
            var lotesPorVencer = await db.Lote
                .AsNoTracking()
                .Include(l => l.Producto)
                .Where(l => l.FechaVencimiento.HasValue && l.FechaVencimiento.Value <= limite
                    && (!idBodega.HasValue || db.DetalleEntrada.Any(d => d.id_lote == l.Id && d.Entrada.id_bodega == idBodega)
                        || db.DetalleAjuste.Any(d => d.id_lote == l.Id && d.Ajuste.id_bodega == idBodega)))
                .OrderBy(l => l.FechaVencimiento)
                .ToListAsync();

            var loteIds = lotesPorVencer.Select(l => l.Id).ToList();
            var entradaOrigenPorLote = loteIds.Count == 0
                ? new Dictionary<long, int>()
                : await db.DetalleEntrada
                    .AsNoTracking()
                    .Where(d => d.id_lote.HasValue && loteIds.Contains(d.id_lote.Value) && (!idBodega.HasValue || d.Entrada.id_bodega == idBodega))
                    .GroupBy(d => d.id_lote!.Value)
                    .Select(g => new
                    {
                        IdLote = g.Key,
                        IdEntrada = g.OrderBy(d => d.id_entrada).Select(d => d.id_entrada).First()
                    })
                    .ToDictionaryAsync(x => x.IdLote, x => x.IdEntrada);

            var ajusteOrigenPorLote = loteIds.Count == 0
                ? new Dictionary<long, int>()
                : await db.DetalleAjuste
                    .AsNoTracking()
                    .Where(d => d.TipoAjuste == TipoAjuste.Aumento
                        && d.id_lote.HasValue
                        && loteIds.Contains(d.id_lote.Value) && (!idBodega.HasValue || d.Ajuste.id_bodega == idBodega))
                    .GroupBy(d => d.id_lote!.Value)
                    .Select(g => new
                    {
                        IdLote = g.Key,
                        IdAjuste = g.OrderBy(d => d.id_ajuste).Select(d => d.id_ajuste).First()
                    })
                    .ToDictionaryAsync(x => x.IdLote, x => x.IdAjuste);

            foreach (var lote in lotesPorVencer)
            {
                var yaVencido = lote.FechaVencimiento!.Value < hoy;
                var diasRestantes = lote.FechaVencimiento.Value.DayNumber - hoy.DayNumber;
                var tieneEntradaOrigen = entradaOrigenPorLote.TryGetValue(lote.Id, out var idEntrada);
                var tieneAjusteOrigen = ajusteOrigenPorLote.TryGetValue(lote.Id, out var idAjuste);

                alertas.Add(new AlertaDto
                {
                    TipoAlerta = "LoteVencimiento",
                    Severidad = yaVencido ? SeveridadAlerta.Critica : SeveridadAlerta.Advertencia,
                    Mensaje = yaVencido
                        ? $"Lote vencido: '{lote.Producto?.Nombre}' — Lote {lote.Codigo} (venció el {lote.FechaVencimiento.Value:dd/MM/yyyy})"
                        : $"Lote por vencer: '{lote.Producto?.Nombre}' — Lote {lote.Codigo} — vence en {diasRestantes} día(s) ({lote.FechaVencimiento.Value:dd/MM/yyyy})",
                    IdReferencia = (int)lote.Id,
                    TipoMovimiento = tieneEntradaOrigen ? "Entrada" : tieneAjusteOrigen ? "Ajuste" : null,
                    IdMovimiento = tieneEntradaOrigen ? idEntrada : tieneAjusteOrigen ? idAjuste : null
                });
            }

            // 2) Alertas desde DetalleEntrada.FechaVencimiento (sin lote, o lote sin fecha)
            var detallesPorVencer = await db.DetalleEntrada
                .AsNoTracking()
                .Include(d => d.Producto)
                .Include(d => d.Lote)
                .Where(d => d.FechaVencimiento.HasValue && (!idBodega.HasValue || d.Entrada.id_bodega == idBodega)
                    && d.FechaVencimiento.Value <= limite
                    && (d.id_lote == null || d.Lote!.FechaVencimiento == null))
                .GroupBy(d => new { d.id_producto, d.FechaVencimiento })
                .Select(g => new
                {
                    g.Key.id_producto,
                    g.Key.FechaVencimiento,
                    IdEntrada = g.OrderBy(d => d.id_entrada).Select(d => d.id_entrada).First(),
                    ProductoNombre = g.First().Producto != null ? g.First().Producto.Nombre : "Desconocido"
                })
                .OrderBy(d => d.FechaVencimiento)
                .ToListAsync();

            foreach (var detalle in detallesPorVencer)
            {
                var fecha = detalle.FechaVencimiento!.Value;
                var yaVencido = fecha < hoy;
                var diasRestantes = fecha.DayNumber - hoy.DayNumber;

                alertas.Add(new AlertaDto
                {
                    TipoAlerta = "LoteVencimiento",
                    Severidad = yaVencido ? SeveridadAlerta.Critica : SeveridadAlerta.Advertencia,
                    Mensaje = yaVencido
                        ? $"Producto vencido: '{detalle.ProductoNombre}' (venció el {fecha:dd/MM/yyyy})"
                        : $"Producto por vencer: '{detalle.ProductoNombre}' — vence en {diasRestantes} día(s) ({fecha:dd/MM/yyyy})",
                    IdReferencia = detalle.id_producto,
                    TipoMovimiento = "Entrada",
                    IdMovimiento = detalle.IdEntrada
                });
            }

            // 3) Alertas desde DetalleAjuste.FechaVencimiento (ajustes de aumento con fecha de vencimiento)
            var ajustesPorVencer = await db.DetalleAjuste
                .AsNoTracking()
                .Include(d => d.Producto)
                .Where(d => d.TipoAjuste == TipoAjuste.Aumento && (!idBodega.HasValue || d.Ajuste.id_bodega == idBodega)
                    && d.FechaVencimiento.HasValue
                    && d.FechaVencimiento.Value <= limite)
                .GroupBy(d => new { d.id_producto, d.FechaVencimiento })
                .Select(g => new
                {
                    g.Key.id_producto,
                    g.Key.FechaVencimiento,
                    IdAjuste = g.OrderBy(d => d.id_ajuste).Select(d => d.id_ajuste).First(),
                    ProductoNombre = g.First().Producto != null ? g.First().Producto.Nombre : "Desconocido"
                })
                .OrderBy(d => d.FechaVencimiento)
                .ToListAsync();

            foreach (var detalle in ajustesPorVencer)
            {
                var fecha = detalle.FechaVencimiento!.Value;
                var yaVencido = fecha < hoy;
                var diasRestantes = fecha.DayNumber - hoy.DayNumber;

                alertas.Add(new AlertaDto
                {
                    TipoAlerta = "LoteVencimiento",
                    Severidad = yaVencido ? SeveridadAlerta.Critica : SeveridadAlerta.Advertencia,
                    Mensaje = yaVencido
                        ? $"Producto vencido (ajuste): '{detalle.ProductoNombre}' (venció el {fecha:dd/MM/yyyy})"
                        : $"Producto por vencer (ajuste): '{detalle.ProductoNombre}' — vence en {diasRestantes} día(s) ({fecha:dd/MM/yyyy})",
                    IdReferencia = detalle.id_producto,
                    TipoMovimiento = "Ajuste",
                    IdMovimiento = detalle.IdAjuste
                });
            }

            return alertas;
        }
    }
}
