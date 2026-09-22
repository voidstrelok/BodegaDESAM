using Microsoft.EntityFrameworkCore;

namespace BodegaDESAM.Services
{
    public class AjusteInventarioService
    {
        private readonly IDbContextFactory<PostgresDataContext> _factory;
        private readonly InventarioService _inventario;
        private readonly AuditService _audit;
        private readonly ProductoSerieService _serieService;

        public AjusteInventarioService(
            IDbContextFactory<PostgresDataContext> factory,
            InventarioService inventario,
            AuditService audit,
            ProductoSerieService serieService)
        {
            _factory = factory;
            _inventario = inventario;
            _audit = audit;
            _serieService = serieService;
        }

        public async Task<List<AjusteInventario>> GetAllAsync(int? idBodega = null)
        {
            using var db = _factory.CreateDbContext();
            var query = db.AjusteInventario
                .Include(a => a.Bodega)
                .Include(a => a.DetalleAjuste)
                    .ThenInclude(d => d.Producto)
                .Include(a => a.DetalleAjuste)
                    .ThenInclude(d => d.Marca)
                .AsQueryable();
            if (idBodega is > 0) query = query.Where(a => a.id_bodega == idBodega);
            return await query.OrderByDescending(a => a.Fecha).ThenByDescending(a => a.Id).ToListAsync();
        }

        public async Task<PagedResult<AjusteInventario>> GetPageAsync(int? idBodega, PageRequest request, CancellationToken cancellationToken = default)
        {
            using var db = _factory.CreateDbContext();
            var query = db.AjusteInventario
                .AsNoTracking()
                .Include(a => a.Bodega)
                .Include(a => a.DetalleAjuste)
                    .ThenInclude(d => d.Producto)
                .Include(a => a.DetalleAjuste)
                    .ThenInclude(d => d.Marca)
                .AsQueryable();
            if (idBodega is > 0) query = query.Where(a => a.id_bodega == idBodega);
            int? id = int.TryParse(request.Search, out var parsedId) ? parsedId : null;
            if (!string.IsNullOrWhiteSpace(request.Search))
                query = query.Where(a =>
                    EF.Functions.ILike(a.Bodega.Nombre, $"%{request.Search}%") ||
                    EF.Functions.ILike(a.Motivo ?? string.Empty, $"%{request.Search}%") ||
                    EF.Functions.ILike(a.Observacion ?? string.Empty, $"%{request.Search}%") ||
                    (id.HasValue && a.Id == id.Value));
            return await query.OrderByDescending(a => a.Fecha).ThenByDescending(a => a.Id).ToPagedAsync(request, cancellationToken);
        }

        public async Task<AjusteInventario?> GetByIdAsync(int id, int? idBodega = null)
        {
            using var db = _factory.CreateDbContext();
            return await db.AjusteInventario
                .Include(a => a.Bodega)
                .Include(a => a.DetalleAjuste)
                    .ThenInclude(d => d.Producto)
                .Include(a => a.DetalleAjuste)
                    .ThenInclude(d => d.Marca)
                .Include(a => a.DetalleAjuste)
                    .ThenInclude(d => d.Modelo)
                .Include(a => a.DetalleAjuste)
                    .ThenInclude(d => d.Lote)
                .Include(a => a.DetalleAjuste)
                    .ThenInclude(d => d.DetalleEntradaOrigen)
                        .ThenInclude(e => e!.Entrada)
                .Include(a => a.DetalleAjuste)
                    .ThenInclude(d => d.DetalleAjusteOrigen)
                        .ThenInclude(o => o!.Ajuste)
                .FirstOrDefaultAsync(a => a.Id == id && (!idBodega.HasValue || a.id_bodega == idBodega));
        }

        /// <summary>
        /// Crea un ajuste de inventario. Valida que las disminuciones no dejen stock negativo.
        /// seriesPorItem: clave = índice del DetalleAjuste (mismo orden que ajuste.DetalleAjuste), valor = lista de series.
        /// </summary>
        public async Task CreateAsync(AjusteInventario ajuste, Dictionary<int, List<string>>? seriesPorItem = null)
        {
            if (string.IsNullOrWhiteSpace(ajuste.IdUsuario))
                throw new ArgumentException("IdUsuario es obligatorio.", nameof(ajuste));

            if (!ajuste.DetalleAjuste.Any())
                throw new InvalidOperationException("El ajuste debe tener al menos un ítem.");

            using var db = _factory.CreateDbContext();
            ValidarAjuste(ajuste);
            await ValidarLotesAsync(db, ajuste);
            await ValidarDisminucionesAsync(db, ajuste);
            db.AjusteInventario.Add(ajuste);
            await db.SaveChangesAsync();

            // Guardar series para ítems de aumento que las tengan
            if (seriesPorItem != null)
            {
                var detalleList = ajuste.DetalleAjuste.ToList();
                foreach (var (idx, series) in seriesPorItem)
                {
                    if (series == null || series.Count == 0) continue;
                    if (idx < 0 || idx >= detalleList.Count) continue;
                    var detalle = detalleList[idx];
                    if (detalle.TipoAjuste != TipoAjuste.Aumento) continue;
                    await _serieService.CrearSeriesDesdeAjusteAsync(detalle.id_producto, detalle.Id, series);
                }
            }

            await _audit.RegistrarAsync(ajuste.IdUsuario, AuditAcciones.Crear, AuditEntidades.AjusteInventario, ajuste.Id,
                new { ajuste.Motivo, items = ajuste.DetalleAjuste.Count }, bodegaId: ajuste.id_bodega);
        }

        private static void ValidarAjuste(AjusteInventario ajuste)
        {
            if (ajuste.id_bodega <= 0)
                throw new InvalidOperationException("Debe seleccionar una bodega.");

            foreach (var detalle in ajuste.DetalleAjuste)
            {
                if (detalle.id_producto <= 0 || detalle.id_marca <= 0 || detalle.Cantidad <= 0)
                    throw new InvalidOperationException("Todos los ítems deben tener producto, marca y cantidad válidos.");

                if (detalle.id_modelo.HasValue && detalle.id_modelo.Value <= 0)
                    throw new InvalidOperationException("El modelo seleccionado no es válido.");
                if (detalle.TipoAjuste == TipoAjuste.Aumento && (!detalle.ValorUnitario.HasValue || detalle.ValorUnitario.Value <= 0))
                    throw new InvalidOperationException("El valor unitario debe ser mayor a 0 en todos los aumentos.");

                if (detalle.TipoAjuste == TipoAjuste.Disminucion &&
                    (detalle.id_detalle_entrada_origen.HasValue == detalle.id_detalle_ajuste_origen.HasValue))
                    throw new InvalidOperationException("Cada disminución debe seleccionar exactamente un origen de stock.");
            }
        }

        private static async Task ValidarLotesAsync(PostgresDataContext db, AjusteInventario ajuste)
        {
            var loteIds = ajuste.DetalleAjuste
                .Where(d => d.id_lote.HasValue)
                .Select(d => d.id_lote!.Value)
                .Distinct()
                .ToList();

            if (loteIds.Count == 0)
                return;

            var lotes = await db.Lote
                .AsNoTracking()
                .Where(l => loteIds.Contains(l.Id))
                .Select(l => new { l.Id, l.id_producto })
                .ToDictionaryAsync(l => l.Id);

            foreach (var detalle in ajuste.DetalleAjuste.Where(d => d.id_lote.HasValue))
            {
                if (!lotes.TryGetValue(detalle.id_lote!.Value, out var lote))
                    throw new InvalidOperationException("El lote seleccionado no existe.");

                if (lote.id_producto != detalle.id_producto)
                    throw new InvalidOperationException("El lote seleccionado no pertenece al producto indicado.");
            }
        }

        /// <summary>
        /// Las salidas descuentan existencias por bodega. Por eso las disminuciones
        /// deben validarse con el mismo ámbito y de forma acumulada por SKU.
        /// </summary>
        private static async Task ValidarDisminucionesAsync(PostgresDataContext db, AjusteInventario ajuste)
        {
            var disminuciones = ajuste.DetalleAjuste
                .Where(d => d.TipoAjuste == TipoAjuste.Disminucion)
                .GroupBy(d => (d.id_producto, d.id_marca, d.id_modelo))
                .ToList();

            if (disminuciones.Count == 0)
                return;

            // Validación por origen: impide descontar de una línea distinta a la seleccionada.
            foreach (var detalle in ajuste.DetalleAjuste.Where(x => x.TipoAjuste == TipoAjuste.Disminucion))
            {
                if (detalle.id_detalle_ajuste_origen.HasValue)
                {
                    var origen = await db.DetalleAjuste.Include(x => x.Ajuste).FirstOrDefaultAsync(x => x.Id == detalle.id_detalle_ajuste_origen && x.TipoAjuste == TipoAjuste.Aumento)
                        ?? throw new InvalidOperationException("El ajuste de origen no existe.");
                    if (origen.Ajuste.id_bodega != ajuste.id_bodega || origen.id_producto != detalle.id_producto || origen.id_marca != detalle.id_marca || origen.id_modelo != detalle.id_modelo || origen.id_lote != detalle.id_lote)
                        throw new InvalidOperationException("El producto no coincide con su ajuste de origen.");
                    var salidasOrigen = await db.DetalleSalida.Where(x => x.id_detalle_ajuste_origen == origen.Id).SumAsync(x => (long?)x.Cantidad) ?? 0;
                    var previas = await db.DetalleAjuste.Where(x => x.id_detalle_ajuste_origen == origen.Id && x.TipoAjuste == TipoAjuste.Disminucion).SumAsync(x => (long?)x.Cantidad) ?? 0;
                    var actuales = ajuste.DetalleAjuste.Where(x => x.id_detalle_ajuste_origen == origen.Id).Sum(x => x.Cantidad);
                    if (salidasOrigen + previas + actuales > origen.Cantidad) throw new InvalidOperationException("Stock insuficiente en el ajuste de origen seleccionado.");
                }
            }

            var entradas = await db.DetalleEntrada
                .AsNoTracking()
                .Where(d => d.Entrada.id_bodega == ajuste.id_bodega)
                .GroupBy(d => new { d.id_producto, d.id_marca, d.id_modelo })
                .Select(g => new { g.Key.id_producto, g.Key.id_marca, g.Key.id_modelo, Cantidad = g.Sum(x => (long)x.Cantidad) })
                .ToListAsync();

            var salidas = await db.DetalleSalida
                .AsNoTracking()
                .Where(d => d.Salida.id_bodega == ajuste.id_bodega)
                .GroupBy(d => new { d.id_producto, d.id_marca, d.id_modelo })
                .Select(g => new { g.Key.id_producto, g.Key.id_marca, g.Key.id_modelo, Cantidad = g.Sum(x => (long)x.Cantidad) })
                .ToListAsync();

            var ajustesPrevios = await db.DetalleAjuste
                .AsNoTracking()
                .Where(d => d.Ajuste.id_bodega == ajuste.id_bodega)
                .GroupBy(d => new { d.id_producto, d.id_marca, d.id_modelo })
                .Select(g => new
                {
                    g.Key.id_producto,
                    g.Key.id_marca,
                    g.Key.id_modelo,
                    Cantidad = g.Sum(x => x.TipoAjuste == TipoAjuste.Aumento ? (long)x.Cantidad : -(long)x.Cantidad)
                })
                .ToListAsync();

            foreach (var grupo in disminuciones)
            {
                var key = grupo.Key;
                var totalEntradas = entradas.FirstOrDefault(x => x.id_producto == key.id_producto && x.id_marca == key.id_marca && x.id_modelo == key.id_modelo)?.Cantidad ?? 0;
                var totalSalidas = salidas.FirstOrDefault(x => x.id_producto == key.id_producto && x.id_marca == key.id_marca && x.id_modelo == key.id_modelo)?.Cantidad ?? 0;
                var netoAjustes = ajustesPrevios.FirstOrDefault(x => x.id_producto == key.id_producto && x.id_marca == key.id_marca && x.id_modelo == key.id_modelo)?.Cantidad ?? 0;
                var solicitado = grupo.Sum(x => x.Cantidad);
                var disponible = totalEntradas - totalSalidas + netoAjustes;

                if (solicitado > disponible)
                {
                    var nombre = await db.Producto.AsNoTracking()
                        .Where(p => p.Id == key.id_producto)
                        .Select(p => p.Nombre)
                        .FirstOrDefaultAsync() ?? $"Producto #{key.id_producto}";
                    throw new InvalidOperationException(
                        $"Stock insuficiente para '{nombre}' en la bodega seleccionada: disponible {disponible}, cantidad a disminuir {solicitado}.");
                }
            }
        }

        public async Task DeleteAsync(int id, string usuarioId)
        {
            using var db = _factory.CreateDbContext();
            var ajuste = await db.AjusteInventario
                .Include(a => a.DetalleAjuste)
                .FirstOrDefaultAsync(a => a.Id == id)
                ?? throw new KeyNotFoundException($"Ajuste #{id} no encontrado.");

            var detalleIds = ajuste.DetalleAjuste.Select(x => x.Id).ToList();
            var tieneConsumos = detalleIds.Count > 0 && (await db.DetalleSalida.AnyAsync(x => x.id_detalle_ajuste_origen.HasValue && detalleIds.Contains(x.id_detalle_ajuste_origen.Value))
                || await db.DetalleAjuste.AnyAsync(x => x.id_detalle_ajuste_origen.HasValue && detalleIds.Contains(x.id_detalle_ajuste_origen.Value)));
            if (tieneConsumos)
                throw new InvalidOperationException("No se puede eliminar el ajuste porque una salida o disminución consume uno de sus aumentos.");

            var series = await db.ProductoSerie.Where(x => x.id_detalle_ajuste.HasValue && detalleIds.Contains(x.id_detalle_ajuste.Value)).ToListAsync();
            db.ProductoSerie.RemoveRange(series);

            db.AjusteInventario.Remove(ajuste);
            await db.SaveChangesAsync();

            await _audit.RegistrarAsync(usuarioId, AuditAcciones.Eliminar, AuditEntidades.AjusteInventario, id, bodegaId: ajuste.id_bodega);
        }
    }
}
