using Microsoft.EntityFrameworkCore;

namespace BodegaDESAM.Services
{
    public class InventarioProductoDto
    {
        public int IdProducto { get; set; }
        public long IdMarca { get; set; }
        public long? IdModelo { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public string? Marca { get; set; }
        public string? Modelo { get; set; }
        public string? Categoria { get; set; }
        public long TotalEntradas { get; set; }
        public long TotalSalidas { get; set; }
        public long NetoAjustes { get; set; }
        public long StockActual { get; set; }
    }

    public class InventarioService
    {
        private readonly IDbContextFactory<PostgresDataContext> _factory;

        public InventarioService(IDbContextFactory<PostgresDataContext> factory)
        {
            _factory = factory;
        }

        public async Task<List<InventarioProductoDto>> GetInventarioActualAsync()
        {
            using var db = _factory.CreateDbContext();

            // Agrupar entradas por producto + marca + modelo
            var entradasPorSKU = await db.DetalleEntrada
                .AsNoTracking()
                .GroupBy(d => new { d.id_producto, d.id_marca, d.id_modelo })
                .Select(g => new
                {
                    g.Key.id_producto,
                    g.Key.id_marca,
                    g.Key.id_modelo,
                    Total = g.Sum(x => (long?)x.Cantidad) ?? 0L
                })
                .ToListAsync();

            // Agrupar salidas por producto + marca + modelo (consistente con entradas)
            var salidasPorSKU = await db.DetalleSalida
                .AsNoTracking()
                .GroupBy(d => new { d.id_producto, d.id_marca, d.id_modelo })
                .Select(g => new
                {
                    g.Key.id_producto,
                    g.Key.id_marca,
                    g.Key.id_modelo,
                    Total = g.Sum(x => (long?)x.Cantidad) ?? 0L
                })
                .ToDictionaryAsync(
                    x => (x.id_producto, x.id_marca, x.id_modelo), 
                    x => x.Total);

            // Agrupar ajustes por producto + marca + modelo
            var ajustesPorSKU = await db.DetalleAjuste
                .AsNoTracking()
                .GroupBy(d => new { d.id_producto, d.id_marca, d.id_modelo })
                .Select(g => new
                {
                    g.Key.id_producto,
                    g.Key.id_marca,
                    g.Key.id_modelo,
                    NetoAjuste = g.Sum(x =>
                        x.TipoAjuste == TipoAjuste.Aumento ? (long)x.Cantidad : -(long)x.Cantidad)
                })
                .ToDictionaryAsync(
                    x => (x.id_producto, x.id_marca, x.id_modelo),
                    x => x.NetoAjuste);

            // Obtener información de productos
            var skuKeys = entradasPorSKU
                .Select(x => (x.id_producto, x.id_marca, x.id_modelo))
                .Union(salidasPorSKU.Keys)
                .Union(ajustesPorSKU.Keys)
                .ToList();
            var entradasPorSku = entradasPorSKU.ToDictionary(
                x => (x.id_producto, x.id_marca, x.id_modelo), x => x.Total);
            var productosIds = skuKeys.Select(e => e.id_producto).Distinct().ToList();
            var productos = await db.Producto
                .AsNoTracking()
                .Where(p => productosIds.Contains(p.Id))
                .Select(p => new { p.Id, p.Nombre, p.id_categoria_producto })
                .ToDictionaryAsync(x => x.Id, x => x);

            // Obtener marcas
            var marcaIds = skuKeys.Select(e => e.id_marca).Distinct().ToList();
            var marcas = await db.Marca
                .AsNoTracking()
                .Where(m => marcaIds.Contains(m.Id))
                .Select(m => new { m.Id, m.Nombre })
                .ToDictionaryAsync(x => x.Id, x => x.Nombre);

            // Obtener modelos
            var modeloIds = skuKeys
                .Where(e => e.id_modelo.HasValue)
                .Select(e => e.id_modelo.Value)
                .Distinct()
                .ToList();
            var modelos = await db.Modelo
                .AsNoTracking()
                .Where(m => modeloIds.Contains(m.Id))
                .Select(m => new { m.Id, m.Nombre })
                .ToDictionaryAsync(x => x.Id, x => x.Nombre);

            // Obtener categorías
            var categoriaIds = productos.Values.Select(p => p.id_categoria_producto).Distinct().ToList();
            var categorias = await db.CategoriaProducto
                .AsNoTracking()
                .Where(c => categoriaIds.Contains(c.Id))
                .Select(c => new { c.Id, c.Nombre })
                .ToDictionaryAsync(x => x.Id, x => x.Nombre);

            var result = new List<InventarioProductoDto>();

            foreach (var skuKey in skuKeys)
            {
                if (!productos.TryGetValue(skuKey.id_producto, out var producto))
                    continue;

                var entradas = entradasPorSku.TryGetValue(skuKey, out var te) ? te : 0L;

                // Buscar salidas con el mismo SKU (Producto + Marca + Modelo)
                var salidas = salidasPorSKU.TryGetValue(
                    skuKey,
                    out var ts) ? ts : 0L;

                // Neto de ajustes (positivo = aumento, negativo = disminución)
                var netoAjuste = ajustesPorSKU.TryGetValue(
                    skuKey,
                    out var aj) ? aj : 0L;

                var stock = entradas - salidas + netoAjuste;

                string? marcaNombre = marcas.TryGetValue(skuKey.id_marca, out var mn) ? mn : null;
                string? modeloNombre = skuKey.id_modelo.HasValue && modelos.TryGetValue(skuKey.id_modelo.Value, out var modn) ? modn : null;
                string? categoriaNombre = categorias.TryGetValue(producto.id_categoria_producto, out var cn) ? cn : null;

                result.Add(new InventarioProductoDto
                {
                    IdProducto = skuKey.id_producto,
                    IdMarca = skuKey.id_marca,
                    IdModelo = skuKey.id_modelo,
                    NombreProducto = producto.Nombre,
                    Marca = marcaNombre,
                    Modelo = modeloNombre,
                    Categoria = categoriaNombre,
                    TotalEntradas = entradas,
                    TotalSalidas = salidas,
                    NetoAjustes = netoAjuste,
                    StockActual = stock
                });
            }

            return result
                .OrderBy(i => i.NombreProducto)
                .ThenBy(i => i.Marca)
                .ThenBy(i => i.Modelo)
                .ToList();
        }

        public async Task<List<ProductoSkuDisponible>> GetSkusDisponiblesAsync(int? idBodega = null)
        {
            using var db = _factory.CreateDbContext();

            // Consulta de entradas, opcionalmente filtrada por bodega
            var entradasQuery = db.DetalleEntrada
                .AsNoTracking()
                .Include(d => d.Entrada)
                .AsQueryable();

            if (idBodega.HasValue && idBodega.Value > 0)
                entradasQuery = entradasQuery.Where(d => d.Entrada.id_bodega == idBodega.Value);

            // Agrupar entradas por SKU + Lote
            var entradasPorLote = await entradasQuery
                .GroupBy(d => new { d.id_producto, d.id_marca, d.id_modelo, d.id_lote })
                .Select(g => new
                {
                    g.Key.id_producto,
                    g.Key.id_marca,
                    g.Key.id_modelo,
                    g.Key.id_lote,
                    Total = g.Sum(x => (long?)x.Cantidad) ?? 0L
                })
                .ToListAsync();

            // Consulta de salidas, opcionalmente filtrada por bodega
            var salidasQuery = db.DetalleSalida
                .AsNoTracking()
                .Include(d => d.Salida)
                .AsQueryable();

            if (idBodega.HasValue && idBodega.Value > 0)
                salidasQuery = salidasQuery.Where(d => d.Salida.id_bodega == idBodega.Value);

            // Agrupar salidas por SKU + Lote
            var salidasPorLote = await salidasQuery
                .GroupBy(d => new { d.id_producto, d.id_marca, d.id_modelo, d.id_lote })
                .Select(g => new
                {
                    g.Key.id_producto,
                    g.Key.id_marca,
                    g.Key.id_modelo,
                    g.Key.id_lote,
                    Total = g.Sum(x => (long?)x.Cantidad) ?? 0L
                })
                .ToDictionaryAsync(
                    x => (x.id_producto, x.id_marca, x.id_modelo, x.id_lote),
                    x => x.Total);

            // Agrupar ajustes por SKU (sin lote — se distribuyen proporcionalmente)
            var ajustesPorSKU = await db.DetalleAjuste
                .AsNoTracking()
                .GroupBy(d => new { d.id_producto, d.id_marca, d.id_modelo })
                .Select(g => new
                {
                    g.Key.id_producto,
                    g.Key.id_marca,
                    g.Key.id_modelo,
                    NetoAjuste = g.Sum(x =>
                        x.TipoAjuste == TipoAjuste.Aumento ? (long)x.Cantidad : -(long)x.Cantidad)
                })
                .ToDictionaryAsync(
                    x => (x.id_producto, x.id_marca, x.id_modelo),
                    x => x.NetoAjuste);

            // Total de entradas por SKU (para distribución proporcional de ajustes)
            var entradaTotalPorSKU = entradasPorLote
                .GroupBy(e => (e.id_producto, e.id_marca, e.id_modelo))
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Total));

            // Obtener información de productos
            var productosIds = entradasPorLote.Select(e => e.id_producto).Distinct().ToList();
            var productos = await db.Producto
                .AsNoTracking()
                .Where(p => productosIds.Contains(p.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Nombre);

            // Obtener marcas
            var marcaIds = entradasPorLote.Select(e => e.id_marca).Distinct().ToList();
            var marcas = await db.Marca
                .AsNoTracking()
                .Where(m => marcaIds.Contains(m.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Nombre);

            // Obtener modelos
            var modeloIds = entradasPorLote
                .Where(e => e.id_modelo.HasValue)
                .Select(e => e.id_modelo!.Value)
                .Distinct()
                .ToList();
            var modelos = await db.Modelo
                .AsNoTracking()
                .Where(m => modeloIds.Contains(m.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Nombre);

            // Obtener lotes
            var loteIds = entradasPorLote
                .Where(e => e.id_lote.HasValue)
                .Select(e => e.id_lote!.Value)
                .Distinct()
                .ToList();
            Dictionary<long, Lote> lotes = new();
            if (loteIds.Count > 0)
            {
                lotes = await db.Lote
                    .AsNoTracking()
                    .Where(l => loteIds.Contains(l.Id))
                    .ToDictionaryAsync(x => x.Id);
            }

            var result = new List<ProductoSkuDisponible>();

            foreach (var entrada in entradasPorLote)
            {
                if (!productos.TryGetValue(entrada.id_producto, out var productoNombre))
                    continue;
                if (!marcas.TryGetValue(entrada.id_marca, out var marcaNombre))
                    continue;

                // Stock del lote específico
                var salidasLote = salidasPorLote.TryGetValue(
                    (entrada.id_producto, entrada.id_marca, entrada.id_modelo, entrada.id_lote),
                    out var ts) ? ts : 0L;

                // Distribuir ajuste del SKU proporcionalmente al peso del lote
                long ajuste = 0;
                var skuTuple = (entrada.id_producto, entrada.id_marca, entrada.id_modelo);
                if (ajustesPorSKU.TryGetValue(skuTuple, out var netoAjuste) && netoAjuste != 0)
                {
                    var skuTotal = entradaTotalPorSKU.TryGetValue(skuTuple, out var st) && st > 0 ? st : 1L;
                    ajuste = (long)Math.Round((double)netoAjuste * entrada.Total / skuTotal);
                }

                var stock = entrada.Total - salidasLote + ajuste;
                if (stock <= 0)
                    continue;

                Lote? lote = entrada.id_lote.HasValue && lotes.TryGetValue(entrada.id_lote.Value, out var l) ? l : null;
                string? modeloNombre = entrada.id_modelo.HasValue && modelos.TryGetValue(entrada.id_modelo.Value, out var mn) ? mn : null;

                result.Add(new ProductoSkuDisponible
                {
                    IdProducto     = entrada.id_producto,
                    NombreProducto = productoNombre,
                    IdMarca        = entrada.id_marca,
                    NombreMarca    = marcaNombre,
                    IdModelo       = entrada.id_modelo,
                    NombreModelo   = modeloNombre,
                    IdLote         = entrada.id_lote,
                    CodigoLote     = lote?.Codigo,
                    FechaVencimiento = lote?.FechaVencimiento,
                    StockDisponible = stock
                });
            }

            return result
                .OrderBy(s => s.NombreProducto)
                .ThenBy(s => s.NombreMarca)
                .ThenBy(s => s.NombreModelo)
                .ThenBy(s => s.FechaVencimiento ?? DateOnly.MaxValue) // FEFO
                .ThenBy(s => s.CodigoLote)
                .ToList();
        }

        /// <summary>
        /// Existencias separadas por línea de entrada. Es el origen que debe
        /// seleccionarse al crear una salida para conservar proveedor y factura.
        /// </summary>
        public async Task<List<ProductoSkuDisponible>> GetEntradasDisponiblesAsync(int? idBodega = null, int? excluirSalidaId = null)
        {
            using var db = _factory.CreateDbContext();

            var entradas = await db.DetalleEntrada
                .AsNoTracking()
                .Include(d => d.Entrada).ThenInclude(e => e.Proveedor)
                .Include(d => d.Producto)
                .Include(d => d.Marca)
                .Include(d => d.Modelo)
                .Include(d => d.Lote)
                .Where(d => !idBodega.HasValue || idBodega.Value <= 0 || d.Entrada.id_bodega == idBodega.Value)
                .OrderBy(d => d.Lote!.FechaVencimiento ?? DateOnly.MaxValue)
                .ThenBy(d => d.Entrada.Fecha)
                .ThenBy(d => d.Id)
                .ToListAsync();

            var detalleIds = entradas.Select(d => d.Id).ToList();
            var consumido = detalleIds.Count == 0
                ? new Dictionary<int, long>()
                : await db.DetalleSalida.AsNoTracking()
                    .Where(d => d.id_detalle_entrada.HasValue && detalleIds.Contains(d.id_detalle_entrada.Value)
                        && (!excluirSalidaId.HasValue || d.id_salida != excluirSalidaId.Value))
                    .GroupBy(d => d.id_detalle_entrada!.Value)
                    .Select(g => new { Id = g.Key, Cantidad = g.Sum(d => (long)d.Cantidad) })
                    .ToDictionaryAsync(x => x.Id, x => x.Cantidad);

            return entradas
                .Select(d => new { Detalle = d, Stock = d.Cantidad - (consumido.TryGetValue(d.Id, out var usado) ? usado : 0L) })
                .Where(x => x.Stock > 0)
                .Select(x => new ProductoSkuDisponible
                {
                    IdDetalleEntrada = x.Detalle.Id,
                    IdEntrada = x.Detalle.id_entrada,
                    IdProducto = x.Detalle.id_producto,
                    NombreProducto = x.Detalle.Producto.Nombre,
                    IdMarca = x.Detalle.id_marca,
                    NombreMarca = x.Detalle.Marca.Nombre,
                    IdModelo = x.Detalle.id_modelo,
                    NombreModelo = x.Detalle.Modelo?.Nombre,
                    IdLote = x.Detalle.id_lote,
                    CodigoLote = x.Detalle.Lote?.Codigo,
                    FechaVencimiento = x.Detalle.FechaVencimiento ?? x.Detalle.Lote?.FechaVencimiento,
                    NombreProveedor = x.Detalle.Entrada.Proveedor.Nombre,
                    NumeroDocumento = x.Detalle.Entrada.NDocumento,
                    StockDisponible = x.Stock
                })
                .ToList();
        }

        /// <summary>Orígenes realmente disponibles: entradas y ajustes de aumento.</summary>
        public async Task<List<ProductoSkuDisponible>> GetOrigenesDisponiblesAsync(int idBodega, int? excluirSalidaId = null)
        {
            using var db = _factory.CreateDbContext();
            var entradas = await GetEntradasDisponiblesAsync(idBodega, excluirSalidaId);

            var aumentos = await db.DetalleAjuste.AsNoTracking()
                .Include(d => d.Ajuste).Include(d => d.Producto).Include(d => d.Marca)
                .Include(d => d.Modelo).Include(d => d.Lote)
                .Where(d => d.Ajuste.id_bodega == idBodega && d.TipoAjuste == TipoAjuste.Aumento)
                .ToListAsync();
            var ids = aumentos.Select(x => x.Id).ToList();
            var usadosSalida = ids.Count == 0 ? new Dictionary<int, long>() : await db.DetalleSalida.AsNoTracking()
                .Where(x => x.id_detalle_ajuste_origen.HasValue && ids.Contains(x.id_detalle_ajuste_origen.Value) && (!excluirSalidaId.HasValue || x.id_salida != excluirSalidaId))
                .GroupBy(x => x.id_detalle_ajuste_origen!.Value).Select(g => new { Id = g.Key, Cantidad = g.Sum(x => (long)x.Cantidad) }).ToDictionaryAsync(x => x.Id, x => x.Cantidad);
            var usadosAjuste = ids.Count == 0 ? new Dictionary<int, long>() : await db.DetalleAjuste.AsNoTracking()
                .Where(x => x.id_detalle_ajuste_origen.HasValue && ids.Contains(x.id_detalle_ajuste_origen.Value) && x.TipoAjuste == TipoAjuste.Disminucion)
                .GroupBy(x => x.id_detalle_ajuste_origen!.Value).Select(g => new { Id = g.Key, Cantidad = g.Sum(x => x.Cantidad) }).ToDictionaryAsync(x => x.Id, x => x.Cantidad);

            entradas.AddRange(aumentos.Select(d => new ProductoSkuDisponible
            {
                IdDetalleAjuste = d.Id, IdProducto = d.id_producto, NombreProducto = d.Producto.Nombre,
                IdMarca = d.id_marca, NombreMarca = d.Marca.Nombre, IdModelo = d.id_modelo,
                NombreModelo = d.Modelo?.Nombre, IdLote = d.id_lote, CodigoLote = d.Lote?.Codigo,
                FechaVencimiento = d.FechaVencimiento ?? d.Lote?.FechaVencimiento,
                StockDisponible = d.Cantidad - (usadosSalida.TryGetValue(d.Id, out var s) ? s : 0) - (usadosAjuste.TryGetValue(d.Id, out var a) ? a : 0)
            }).Where(x => x.StockDisponible > 0));
            return entradas.OrderBy(x => x.NombreProducto).ThenBy(x => x.NombreMarca).ThenBy(x => x.DisplayText).ToList();
        }

    }
}
