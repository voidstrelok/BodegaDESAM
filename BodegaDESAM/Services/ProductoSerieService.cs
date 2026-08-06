using Microsoft.EntityFrameworkCore;

namespace BodegaDESAM.Services
{
    public sealed record SalidaSerieSeleccion(
        int IdProducto,
        int? IdDetalleEntrada,
        int? IdDetalleAjusteOrigen,
        IEnumerable<string> Series);

    public class ProductoSerieService
    {
        private readonly IDbContextFactory<PostgresDataContext> _factory;

        public ProductoSerieService(IDbContextFactory<PostgresDataContext> factory)
        {
            _factory = factory;
        }

        public async Task<List<string>> GetDisponiblesAsync(int idProducto)
        {
            using var db = _factory.CreateDbContext();

            return await db.ProductoSerie
                .AsNoTracking()
                .Where(s => s.id_producto == idProducto && s.id_salida == null)
                .OrderBy(s => s.Serie)
                .Select(s => s.Serie)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene las series disponibles para el origen seleccionado de una salida.
        /// </summary>
        public async Task<List<string>> GetDisponiblesPorSkuAsync(
            int idProducto,
            int? idDetalleEntrada = null,
            int? idDetalleAjusteOrigen = null)
        {
            using var db = _factory.CreateDbContext();

            return await db.ProductoSerie
                .AsNoTracking()
                .Where(s => s.id_producto == idProducto 
                    && s.id_salida == null
                    && (!idDetalleEntrada.HasValue || s.id_detalle_entrada == idDetalleEntrada.Value)
                    && (!idDetalleAjusteOrigen.HasValue || s.id_detalle_ajuste == idDetalleAjusteOrigen.Value))
                .OrderBy(s => s.Serie)
                .Select(s => s.Serie)
                .ToListAsync();
        }

        public async Task<List<ProductoSerie>> CrearSeriesAsync(int idProducto, int idDetalleEntrada, IEnumerable<string> series)
        {
            var seriesList = series
                .Select(s => (s ?? string.Empty).Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            using var db = _factory.CreateDbContext();

            // Duplicados en el lote
            var dupLote = seriesList
                .GroupBy(s => s, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(g => g.Count() > 1);

            if (dupLote != null)
                throw new InvalidOperationException($"N° de serie duplicado en el ingreso: {dupLote.Key}");

            // Duplicados ya existentes para el mismo producto
            var existentes = await db.ProductoSerie
                .AsNoTracking()
                .Where(x => x.id_producto == idProducto)
                .Select(x => x.Serie)
                .ToListAsync();

            var setExistentes = new HashSet<string>(existentes, StringComparer.OrdinalIgnoreCase);
            var repetido = seriesList.FirstOrDefault(s => setExistentes.Contains(s));
            if (repetido != null)
                throw new InvalidOperationException($"El N° de serie ya existe para este producto: {repetido}");

            var entities = seriesList.Select(s => new ProductoSerie
            {
                id_producto = idProducto,
                id_detalle_entrada = idDetalleEntrada,
                Serie = s,
                id_salida = null
            }).ToList();

            db.ProductoSerie.AddRange(entities);
            await db.SaveChangesAsync();
            return entities;
        }

        public async Task SincronizarSeriesEntradaAsync(int idProducto, int idDetalleEntrada, IEnumerable<string> series)
        {
            var seriesList = series
                .Select(s => (s ?? string.Empty).Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            var duplicada = seriesList
                .GroupBy(s => s, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(g => g.Count() > 1);
            if (duplicada != null)
                throw new InvalidOperationException($"N° de serie duplicado en el ingreso: {duplicada.Key}");

            using var db = _factory.CreateDbContext();
            var existentes = await db.ProductoSerie
                .Where(s => s.id_detalle_entrada == idDetalleEntrada)
                .ToListAsync();

            if (existentes.Any(s => s.id_salida != null))
                throw new InvalidOperationException("No se pueden modificar series que ya están asociadas a una salida.");

            var solicitadas = new HashSet<string>(seriesList, StringComparer.OrdinalIgnoreCase);
            var conflictos = await db.ProductoSerie
                .AsNoTracking()
                .Where(s => s.id_producto == idProducto && s.id_detalle_entrada != idDetalleEntrada)
                .Select(s => s.Serie)
                .ToListAsync();
            var conflicto = seriesList.FirstOrDefault(s => conflictos.Contains(s, StringComparer.OrdinalIgnoreCase));
            if (conflicto != null)
                throw new InvalidOperationException($"El N° de serie ya existe para este producto: {conflicto}");

            db.ProductoSerie.RemoveRange(existentes.Where(s => !solicitadas.Contains(s.Serie)));
            foreach (var existente in existentes.Where(s => solicitadas.Contains(s.Serie)))
                existente.id_producto = idProducto;

            var existentesSet = new HashSet<string>(existentes.Select(s => s.Serie), StringComparer.OrdinalIgnoreCase);
            db.ProductoSerie.AddRange(seriesList
                .Where(s => !existentesSet.Contains(s))
                .Select(s => new ProductoSerie
                {
                    id_producto = idProducto,
                    id_detalle_entrada = idDetalleEntrada,
                    Serie = s
                }));

            await db.SaveChangesAsync();
        }

        public async Task<List<ProductoSerie>> CrearSeriesDesdeAjusteAsync(int idProducto, int idDetalleAjuste, IEnumerable<string> series)
        {
            var seriesList = series
                .Select(s => (s ?? string.Empty).Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            using var db = _factory.CreateDbContext();

            var dupLote = seriesList
                .GroupBy(s => s, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(g => g.Count() > 1);

            if (dupLote != null)
                throw new InvalidOperationException($"N° de serie duplicado en el ajuste: {dupLote.Key}");

            var existentes = await db.ProductoSerie
                .AsNoTracking()
                .Where(x => x.id_producto == idProducto)
                .Select(x => x.Serie)
                .ToListAsync();

            var setExistentes = new HashSet<string>(existentes, StringComparer.OrdinalIgnoreCase);
            var repetido = seriesList.FirstOrDefault(s => setExistentes.Contains(s));
            if (repetido != null)
                throw new InvalidOperationException($"El N° de serie ya existe para este producto: {repetido}");

            var entities = seriesList.Select(s => new ProductoSerie
            {
                id_producto = idProducto,
                id_detalle_ajuste = idDetalleAjuste,
                Serie = s,
                id_salida = null
            }).ToList();

            db.ProductoSerie.AddRange(entities);
            await db.SaveChangesAsync();
            return entities;
        }

        public async Task AsociarASalidaAsync(
            int idSalida,
            int idProducto,
            IEnumerable<string> series,
            int? idDetalleEntrada = null,
            int? idDetalleAjusteOrigen = null)
        {
            var seriesList = series
                .Select(s => (s ?? string.Empty).Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            using var db = _factory.CreateDbContext();

            var disponibles = await db.ProductoSerie
                .Where(s => s.id_producto == idProducto && s.id_salida == null && seriesList.Contains(s.Serie)
                    && (!idDetalleEntrada.HasValue || s.id_detalle_entrada == idDetalleEntrada.Value)
                    && (!idDetalleAjusteOrigen.HasValue || s.id_detalle_ajuste == idDetalleAjusteOrigen.Value))
                .ToListAsync();

            if (disponibles.Count != seriesList.Count)
                throw new InvalidOperationException("Una o más series no están disponibles.");

            foreach (var s in disponibles)
                s.id_salida = idSalida;

            await db.SaveChangesAsync();
        }

        public async Task SincronizarSalidaAsync(int idSalida, IEnumerable<SalidaSerieSeleccion> seleccion)
        {
            var solicitadas = seleccion
                .SelectMany(x => x.Series.Select(serie => new
                {
                    x.IdProducto,
                    x.IdDetalleEntrada,
                    x.IdDetalleAjusteOrigen,
                    Serie = (serie ?? string.Empty).Trim()
                }))
                .Where(x => !string.IsNullOrWhiteSpace(x.Serie))
                .ToList();

            var duplicada = solicitadas
                .GroupBy(x => (x.IdProducto, x.IdDetalleEntrada, x.IdDetalleAjusteOrigen, Serie: x.Serie.ToUpperInvariant()))
                .FirstOrDefault(g => g.Count() > 1);
            if (duplicada != null)
                throw new InvalidOperationException("Una serie no puede seleccionarse más de una vez.");

            using var db = _factory.CreateDbContext();
            var actuales = await db.ProductoSerie.Where(s => s.id_salida == idSalida).ToListAsync();
            var candidatas = await db.ProductoSerie
                .Where(s => s.id_salida == null || s.id_salida == idSalida)
                .ToListAsync();

            var idsSolicitados = new HashSet<int>();
            foreach (var item in solicitadas)
            {
                var serie = candidatas.FirstOrDefault(s => s.id_producto == item.IdProducto
                    && s.id_detalle_entrada == item.IdDetalleEntrada
                    && s.id_detalle_ajuste == item.IdDetalleAjusteOrigen
                    && string.Equals(s.Serie, item.Serie, StringComparison.OrdinalIgnoreCase));
                if (serie == null)
                    throw new InvalidOperationException($"La serie '{item.Serie}' ya no está disponible.");
                idsSolicitados.Add(serie.Id);
            }

            foreach (var serie in actuales)
                serie.id_salida = null;
            foreach (var serie in candidatas.Where(s => idsSolicitados.Contains(s.Id)))
                serie.id_salida = idSalida;

            await db.SaveChangesAsync();
        }

        public async Task<List<ProductoSerie>> GetSeriesPorSalidaAsync(int idSalida)
        {
            using var db = _factory.CreateDbContext();

            return await db.ProductoSerie
                .AsNoTracking()
                .Where(s => s.id_salida == idSalida)
                .OrderBy(s => s.id_producto)
                .ThenBy(s => s.Serie)
                .ToListAsync();
        }

        public async Task<List<ProductoSerie>> GetSeriesPorAjusteAsync(int idAjuste)
        {
            using var db = _factory.CreateDbContext();
            return await db.ProductoSerie.AsNoTracking().Include(x => x.DetalleAjuste)
                .Where(x => x.id_detalle_ajuste.HasValue && x.DetalleAjuste!.id_ajuste == idAjuste)
                .OrderBy(x => x.Serie).ToListAsync();
        }
    }
}
