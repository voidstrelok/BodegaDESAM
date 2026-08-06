using Microsoft.EntityFrameworkCore;

namespace BodegaDESAM.Services
{
    public class SalidaService
    {
        private readonly IDbContextFactory<PostgresDataContext> _factory;
        private readonly AuditService _audit;

        public SalidaService(IDbContextFactory<PostgresDataContext> factory, AuditService audit)
        {
            _factory = factory;
            _audit = audit;
        }

        public async Task<Dictionary<int, long>> GetEntradasPorProductoAsync(List<int> productoIds)
        {
            using var db = _factory.CreateDbContext();

            return await db.DetalleEntrada
                .AsNoTracking()
                .Where(d => productoIds.Contains(d.id_producto))
                .GroupBy(d => d.id_producto)
                .Select(g => new { IdProducto = g.Key, Total = g.Sum(x => (long?)x.Cantidad) ?? 0L })
                .ToDictionaryAsync(x => x.IdProducto, x => x.Total);
        }

        public async Task<Dictionary<int, long>> GetSalidasPorProductoAsync(List<int> productoIds, int? excluirSalidaId)
        {
            using var db = _factory.CreateDbContext();

            var query = db.DetalleSalida
                .AsNoTracking()
                .Where(d => productoIds.Contains(d.id_producto));

            if (excluirSalidaId != null)
                query = query.Where(d => d.id_salida != excluirSalidaId.Value);

            return await query
                .GroupBy(d => d.id_producto)
                .Select(g => new { IdProducto = g.Key, Total = g.Sum(x => (long?)x.Cantidad) ?? 0L })
                .ToDictionaryAsync(x => x.IdProducto, x => x.Total);
        }

        public async Task<List<Salida>> GetAllAsync()
        {
            using var db = _factory.CreateDbContext();
            return await db.Salida
                .AsNoTracking()
                .Include(s => s.Bodega)
                .Include(s => s.EStablecimiento)
                .Include(s => s.DetalleSalida)
                .OrderByDescending(s => s.Id)
                .ToListAsync();
        }

        public async Task<Salida?> GetByIdAsync(int id)
        {
            using var db = _factory.CreateDbContext();
            return await db.Salida
                .Include(s => s.Bodega)
                .Include(s => s.EStablecimiento)
                .Include(s => s.ProductoSeries)
                .Include(s => s.DetalleSalida)
                    .ThenInclude(d => d.Producto)
                    .ThenInclude(p => p.CategoriaProducto)
                .Include(s => s.DetalleSalida)
                    .ThenInclude(d => d.Marca)
                .Include(s => s.DetalleSalida)
                    .ThenInclude(d => d.Modelo)
                .Include(s => s.DetalleSalida)
                    .ThenInclude(d => d.Lote)
                        .ThenInclude(l => l!.DetalleEntradas)
                            .ThenInclude(de => de.Entrada)
                                .ThenInclude(e => e.Proveedor)
                .Include(s => s.DetalleSalida)
                    .ThenInclude(d => d.DetalleEntrada)
                        .ThenInclude(de => de!.Entrada)
                            .ThenInclude(e => e.Proveedor)
                .Include(s => s.DetalleSalida)
                    .ThenInclude(d => d.DetalleAjusteOrigen)
                        .ThenInclude(da => da!.Ajuste)
                            
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task CreateAsync(Salida salida)
        {
            if (string.IsNullOrWhiteSpace(salida.IdUsuario))
                throw new ArgumentException("IdUsuario es obligatorio.", nameof(salida));

            using var db = _factory.CreateDbContext();

            await ValidarStockDisponibleAsync(db, salida);
            db.Salida.Add(salida);
            await db.SaveChangesAsync();
            await _audit.RegistrarAsync(salida.IdUsuario, AuditAcciones.Crear, AuditEntidades.Salida, salida.Id,
                new { salida.id_establecimiento, Items = salida.DetalleSalida.Count });
        }

        public Task CreateAsync(Salida salida, string idUsuario)
        {
            salida.IdUsuario = idUsuario;
            return CreateAsync(salida);
        }

        public async Task UpdateAsync(Salida salida)
        {
            if (string.IsNullOrWhiteSpace(salida.IdUsuario))
                throw new ArgumentException("IdUsuario es obligatorio.", nameof(salida));

            using var db = _factory.CreateDbContext();

            await ValidarStockDisponibleAsync(db, salida);

            var persistida = await db.Salida
                .Include(s => s.DetalleSalida)
                .FirstOrDefaultAsync(s => s.Id == salida.Id)
                ?? throw new InvalidOperationException("La salida que intenta editar ya no existe.");

            persistida.id_bodega = salida.id_bodega;
            persistida.id_establecimiento = salida.id_establecimiento;
            persistida.Fecha = salida.Fecha;
            persistida.Solicitante = salida.Solicitante;
            persistida.Observacion = salida.Observacion;

            var idsDetalle = salida.DetalleSalida.Where(d => d.Id > 0).Select(d => d.Id).ToHashSet();
            db.DetalleSalida.RemoveRange(persistida.DetalleSalida.Where(d => !idsDetalle.Contains(d.Id)));

            foreach (var detalle in salida.DetalleSalida)
            {
                var destino = persistida.DetalleSalida.FirstOrDefault(d => d.Id == detalle.Id);
                if (destino == null)
                {
                    db.DetalleSalida.Add(new DetalleSalida
                    {
                        id_salida = persistida.Id,
                        id_producto = detalle.id_producto,
                        id_marca = detalle.id_marca,
                        id_modelo = detalle.id_modelo,
                        id_lote = detalle.id_lote,
                        id_ubicacion = detalle.id_ubicacion,
                        id_detalle_entrada = detalle.id_detalle_entrada,
                        id_detalle_ajuste_origen = detalle.id_detalle_ajuste_origen,
                        Cantidad = detalle.Cantidad
                    });
                    continue;
                }

                destino.id_producto = detalle.id_producto;
                destino.id_marca = detalle.id_marca;
                destino.id_modelo = detalle.id_modelo;
                destino.id_lote = detalle.id_lote;
                destino.id_ubicacion = detalle.id_ubicacion;
                destino.id_detalle_entrada = detalle.id_detalle_entrada;
                destino.id_detalle_ajuste_origen = detalle.id_detalle_ajuste_origen;
                destino.Cantidad = detalle.Cantidad;
            }
            await db.SaveChangesAsync();
            await _audit.RegistrarAsync(persistida.IdUsuario, AuditAcciones.Editar, AuditEntidades.Salida, salida.Id,
                new { salida.id_establecimiento });
        }

        private static async Task ValidarStockDisponibleAsync(PostgresDataContext db, Salida salida)
        {
            if (salida.DetalleSalida == null || salida.DetalleSalida.Count == 0)
                throw new InvalidOperationException("Debe agregar al menos un producto al detalle.");

            // Los selectores de origen pueden representar una FK no aplicable como 0.
            // Para Identity/EF, solo un valor positivo constituye un origen válido.
            foreach (var detalle in salida.DetalleSalida)
            {
                if (detalle.id_detalle_entrada <= 0)
                    detalle.id_detalle_entrada = null;
                if (detalle.id_detalle_ajuste_origen <= 0)
                    detalle.id_detalle_ajuste_origen = null;
            }

            if (salida.DetalleSalida.Any(d => d.Cantidad <= 0 || (d.id_detalle_entrada.HasValue == d.id_detalle_ajuste_origen.HasValue)))
                throw new InvalidOperationException("Debe seleccionar exactamente un origen de stock para cada producto.");

            var requeridoPorEntrada = salida.DetalleSalida.Where(d => d.id_detalle_entrada.HasValue)
                .GroupBy(d => d.id_detalle_entrada!.Value)
                .ToDictionary(g => g.Key, g => g.Sum(d => (long)d.Cantidad));
            var detalleIds = requeridoPorEntrada.Keys.ToList();
            var entradasOrigen = await db.DetalleEntrada
                .AsNoTracking()
                .Include(d => d.Entrada)
                .Include(d => d.Producto)
                .Where(d => detalleIds.Contains(d.Id))
                .ToDictionaryAsync(d => d.Id);

            if (entradasOrigen.Count != detalleIds.Count)
                throw new InvalidOperationException("Una de las entradas de origen ya no existe.");

            var consumidoPorEntrada = await db.DetalleSalida
                .AsNoTracking()
                .Where(d => d.id_detalle_entrada.HasValue && detalleIds.Contains(d.id_detalle_entrada.Value)
                    && d.id_salida != salida.Id)
                .GroupBy(d => d.id_detalle_entrada!.Value)
                .Select(g => new { Id = g.Key, Cantidad = g.Sum(d => (long)d.Cantidad) })
                .ToDictionaryAsync(x => x.Id, x => x.Cantidad);

            foreach (var item in salida.DetalleSalida.Where(d => d.id_detalle_entrada.HasValue))
            {
                var entradaOrigen = entradasOrigen[item.id_detalle_entrada!.Value];
                if (entradaOrigen.Entrada.id_bodega != salida.id_bodega ||
                    entradaOrigen.id_producto != item.id_producto || entradaOrigen.id_marca != item.id_marca ||
                    entradaOrigen.id_modelo != item.id_modelo || entradaOrigen.id_lote != item.id_lote)
                    throw new InvalidOperationException("El producto de salida no coincide con su entrada de origen.");
            }

            foreach (var req in requeridoPorEntrada)
            {
                var entradaOrigen = entradasOrigen[req.Key];
                var disponible = entradaOrigen.Cantidad - (consumidoPorEntrada.TryGetValue(req.Key, out var usado) ? usado : 0L);
                if (req.Value > disponible)
                    throw new InvalidOperationException($"Stock insuficiente para '{entradaOrigen.Producto.Nombre}' de la factura {entradaOrigen.Entrada.NDocumento}. Disponible: {disponible}. Solicitado: {req.Value}.");
            }

            var requeridoPorAjuste = salida.DetalleSalida.Where(d => d.id_detalle_ajuste_origen.HasValue)
                .GroupBy(d => d.id_detalle_ajuste_origen!.Value)
                .ToDictionary(g => g.Key, g => g.Sum(x => (long)x.Cantidad));
            var ajusteIds = requeridoPorAjuste.Keys.ToList();
            if (ajusteIds.Count > 0)
            {
                var origenes = await db.DetalleAjuste.AsNoTracking().Include(d => d.Ajuste).Include(d => d.Producto)
                    .Where(d => ajusteIds.Contains(d.Id) && d.TipoAjuste == TipoAjuste.Aumento).ToDictionaryAsync(d => d.Id);
                if (origenes.Count != ajusteIds.Count) throw new InvalidOperationException("Uno de los ajustes de origen ya no existe.");
                var usadas = await db.DetalleSalida.AsNoTracking().Where(d => d.id_detalle_ajuste_origen.HasValue && ajusteIds.Contains(d.id_detalle_ajuste_origen.Value) && d.id_salida != salida.Id)
                    .GroupBy(d => d.id_detalle_ajuste_origen!.Value).Select(g => new { Id = g.Key, Cantidad = g.Sum(x => (long)x.Cantidad) }).ToDictionaryAsync(x => x.Id, x => x.Cantidad);
                var disminuidas = await db.DetalleAjuste.AsNoTracking().Where(d => d.id_detalle_ajuste_origen.HasValue && ajusteIds.Contains(d.id_detalle_ajuste_origen.Value) && d.TipoAjuste == TipoAjuste.Disminucion)
                    .GroupBy(d => d.id_detalle_ajuste_origen!.Value).Select(g => new { Id = g.Key, Cantidad = g.Sum(x => x.Cantidad) }).ToDictionaryAsync(x => x.Id, x => x.Cantidad);
                foreach (var req in requeridoPorAjuste)
                {
                    var origen = origenes[req.Key];
                    var items = salida.DetalleSalida.Where(x => x.id_detalle_ajuste_origen == req.Key);
                    if (origen.Ajuste.id_bodega != salida.id_bodega || items.Any(x => x.id_producto != origen.id_producto || x.id_marca != origen.id_marca || x.id_modelo != origen.id_modelo || x.id_lote != origen.id_lote))
                        throw new InvalidOperationException("El producto de salida no coincide con su ajuste de origen.");
                    var disponible = origen.Cantidad - (usadas.TryGetValue(req.Key, out var usada) ? usada : 0) - (disminuidas.TryGetValue(req.Key, out var disminuida) ? disminuida : 0);
                    if (req.Value > disponible) throw new InvalidOperationException($"Stock insuficiente para '{origen.Producto.Nombre}' del ajuste #{origen.id_ajuste}. Disponible: {disponible}.");
                }
            }

        }

        public async Task<bool> DeleteAsync(long id)
        {
            using var db = _factory.CreateDbContext();

            var salida = await db.Salida
                .Include(s => s.DetalleSalida)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (salida is null)
                return false;

            // Desvincular series asociadas a esta salida antes de eliminar
            var series = await db.ProductoSerie
                .Where(ps => ps.id_salida == (int)id)
                .ToListAsync();

            foreach (var serie in series)
                serie.id_salida = null;

            if (series.Count > 0)
                await db.SaveChangesAsync();

            if (salida.DetalleSalida is not null && salida.DetalleSalida.Count > 0)
                db.DetalleSalida.RemoveRange(salida.DetalleSalida);

            db.Salida.Remove(salida);
            await db.SaveChangesAsync();
            await _audit.RegistrarAsync(salida.IdUsuario, AuditAcciones.Eliminar, AuditEntidades.Salida, (int)id, null);
            return true;
        }
    }
}
