using Microsoft.EntityFrameworkCore;
using System;

namespace BodegaDESAM.Services
{
    public class EntradaService
    {
        private readonly IDbContextFactory<PostgresDataContext> _factory;
        private readonly AuditService _audit;

        public EntradaService(IDbContextFactory<PostgresDataContext> db, AuditService audit)
        {
            _factory = db;
            _audit = audit;
        }

        public async Task<List<Entrada>> GetAllAsync()
        {
            using var db = _factory.CreateDbContext();
            return await db.Entrada
                .Include(e => e.Bodega)
                .Include(e => e.Proveedor)
                .Include(e => e.DetalleEntrada)
                .OrderBy(e => e.Id)
                .ToListAsync();
        }

        public async Task<Entrada?> GetByIdAsync(int id)
        {
            using var db = _factory.CreateDbContext();
            var entrada = await db.Entrada
                .Include(e => e.Bodega)
                .Include(e => e.Proveedor)
                .Include(e => e.DetalleEntrada)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (entrada is null)
                return null;

            var detalle = await db.DetalleEntrada
                .Where(d => d.id_entrada == entrada.Id)
                .Include(d => d.Producto)
                    .ThenInclude(p => p.CategoriaProducto)
                .Include(d => d.Marca)
                .Include(d => d.Modelo)
                .Include(d => d.Lote)
                .Include(d => d.ProductoSeries)
                .ToListAsync();

            if (detalle.Count > 0)
                entrada.DetalleEntrada = detalle;

            return entrada;
        }

        public async Task CreateAsync(Entrada entrada)
        {
            if (string.IsNullOrWhiteSpace(entrada.IdUsuario))
                throw new ArgumentException("IdUsuario es obligatorio.", nameof(entrada));

            using var db = _factory.CreateDbContext();

            ValidarEntrada(entrada);
            db.Entrada.Add(entrada);
            await db.SaveChangesAsync();
            await _audit.RegistrarAsync(entrada.IdUsuario, AuditAcciones.Crear, AuditEntidades.Entrada, entrada.Id,
                new { entrada.NDocumento, entrada.id_proveedor, Items = entrada.DetalleEntrada.Count });
        }

        public Task CreateAsync(Entrada entrada, string idUsuario)
        {
            entrada.IdUsuario = idUsuario;
            return CreateAsync(entrada);
        }


        public async Task UpdateAsync(Entrada entrada)
        {
            if (string.IsNullOrWhiteSpace(entrada.IdUsuario))
                throw new ArgumentException("IdUsuario es obligatorio.", nameof(entrada));

            using var db = _factory.CreateDbContext();

            ValidarEntrada(entrada);

            // No adjuntar el grafo recibido con Update(). La pantalla de edición es
            // desconectada y sus detalles pueden traer una referencia a otra instancia
            // de Entrada con la misma clave.
            var existente = await db.Entrada
                .Include(e => e.DetalleEntrada)
                    .ThenInclude(d => d.ProductoSeries)
                .FirstOrDefaultAsync(e => e.Id == entrada.Id);

            if (existente is null)
                throw new InvalidOperationException("La entrada no existe o fue eliminada.");

            var detalleIds = existente.DetalleEntrada.Select(d => d.Id).ToList();
            var tieneSalidas = detalleIds.Count > 0 && await db.DetalleSalida
                .AnyAsync(d => d.id_detalle_entrada.HasValue && detalleIds.Contains(d.id_detalle_entrada.Value));

            if (tieneSalidas && DetalleFueModificado(existente.DetalleEntrada, entrada.DetalleEntrada))
                throw new InvalidOperationException("No se puede modificar el detalle de una entrada que ya abastece salidas.");

            existente.Fecha = entrada.Fecha;
            existente.id_proveedor = entrada.id_proveedor;
            existente.NDocumento = entrada.NDocumento;
            existente.Observacion = entrada.Observacion;

            // La bodega también determina la validez de la salida contra su entrada de origen.
            if (tieneSalidas && existente.id_bodega != entrada.id_bodega)
                throw new InvalidOperationException("No se puede cambiar la bodega de una entrada que ya abastece salidas.");

            existente.id_bodega = entrada.id_bodega;

            var nuevosDetalles = !tieneSalidas
                ? SincronizarDetalle(db, existente, entrada.DetalleEntrada)
                : [];

            await db.SaveChangesAsync();

            // La pantalla necesita las claves generadas para sincronizar sus series.
            foreach (var (recibido, creado) in nuevosDetalles)
                recibido.Id = creado.Id;
            await _audit.RegistrarAsync(entrada.IdUsuario, AuditAcciones.Editar, AuditEntidades.Entrada, entrada.Id,
                new { entrada.NDocumento, entrada.id_proveedor });
        }

        public async Task<bool> TieneSalidasRelacionadasAsync(int entradaId)
        {
            using var db = _factory.CreateDbContext();

            return await db.DetalleSalida.AnyAsync(d =>
                d.id_detalle_entrada.HasValue &&
                d.DetalleEntrada != null &&
                d.DetalleEntrada.id_entrada == entradaId);
        }

        private static bool DetalleFueModificado(IEnumerable<DetalleEntrada> actual, IEnumerable<DetalleEntrada> recibido)
        {
            var actualPorId = actual.ToDictionary(d => d.Id);
            var recibidoLista = recibido.ToList();

            if (actualPorId.Count != recibidoLista.Count || recibidoLista.Any(d => d.Id == 0 || !actualPorId.ContainsKey(d.Id)))
                return true;

            return recibidoLista.Any(d =>
            {
                var original = actualPorId[d.Id];
                return original.id_producto != d.id_producto ||
                    original.id_marca != d.id_marca ||
                    original.id_modelo != d.id_modelo ||
                    original.id_lote != d.id_lote ||
                    original.id_ubicacion != d.id_ubicacion ||
                    original.Cantidad != d.Cantidad ||
                    original.FechaVencimiento != d.FechaVencimiento;
            });
        }

        private static List<(DetalleEntrada Recibido, DetalleEntrada Creado)> SincronizarDetalle(PostgresDataContext db, Entrada existente, IEnumerable<DetalleEntrada> recibido)
        {
            var recibidos = recibido.ToList();
            var existentesPorId = existente.DetalleEntrada.ToDictionary(d => d.Id);
            var nuevos = new List<(DetalleEntrada Recibido, DetalleEntrada Creado)>();

            if (recibidos.Where(d => d.Id != 0).Any(d => !existentesPorId.ContainsKey(d.Id)))
                throw new InvalidOperationException("El detalle de la entrada no coincide con el registro existente.");

            foreach (var original in existente.DetalleEntrada.ToList())
            {
                if (recibidos.All(d => d.Id != original.Id))
                {
                    db.ProductoSerie.RemoveRange(original.ProductoSeries);
                    db.DetalleEntrada.Remove(original);
                }
            }

            foreach (var item in recibidos)
            {
                if (item.Id == 0)
                {
                    var creado = new DetalleEntrada
                    {
                        id_producto = item.id_producto,
                        id_marca = item.id_marca,
                        id_modelo = item.id_modelo,
                        id_lote = item.id_lote,
                        id_ubicacion = item.id_ubicacion,
                        Cantidad = item.Cantidad,
                        FechaVencimiento = item.FechaVencimiento
                    };
                    existente.DetalleEntrada.Add(creado);
                    nuevos.Add((item, creado));
                    continue;
                }

                var original = existentesPorId[item.Id];
                original.id_producto = item.id_producto;
                original.id_marca = item.id_marca;
                original.id_modelo = item.id_modelo;
                original.id_lote = item.id_lote;
                original.id_ubicacion = item.id_ubicacion;
                original.Cantidad = item.Cantidad;
                original.FechaVencimiento = item.FechaVencimiento;
            }

            return nuevos;
        }

        private static void ValidarEntrada(Entrada entrada)
        {
            if (entrada.id_proveedor <= 0)
                throw new InvalidOperationException("Debe seleccionar un proveedor.");

            if (string.IsNullOrWhiteSpace(entrada.NDocumento))
                throw new InvalidOperationException("El N° de documento es obligatorio.");

            if (entrada.DetalleEntrada == null || entrada.DetalleEntrada.Count == 0)
                throw new InvalidOperationException("Debe agregar al menos un producto al detalle.");

            foreach (var d in entrada.DetalleEntrada)
            {
                if (d.id_producto <= 0)
                    throw new InvalidOperationException("Debe seleccionar un producto en todas las filas del detalle.");

                if (d.id_marca <= 0)
                    throw new InvalidOperationException("Debe seleccionar una marca en todas las filas del detalle.");

                if (d.id_modelo.HasValue && d.id_modelo.Value <= 0)
                    throw new InvalidOperationException("El modelo seleccionado no es válido.");

                if (d.Cantidad <= 0)
                    throw new InvalidOperationException("La cantidad debe ser mayor a 0 en todas las filas del detalle.");
            }
        }

        public async Task<bool> DeleteAsync(long id)
        {
            using var db = _factory.CreateDbContext();

            var entrada = await db.Entrada
                .Include(e => e.DetalleEntrada)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (entrada is null)
                return false;

            if (entrada.DetalleEntrada is not null && entrada.DetalleEntrada.Count > 0)
            {
                var detalleIds = entrada.DetalleEntrada.Select(d => d.Id).ToList();

                var salidasRelacionadas = await db.DetalleSalida
                    .AnyAsync(d => d.id_detalle_entrada.HasValue && detalleIds.Contains(d.id_detalle_entrada.Value));
                if (salidasRelacionadas)
                    throw new InvalidOperationException("No se puede eliminar esta entrada porque tiene una o más salidas asociadas. Sus productos ya fueron despachados; primero debe eliminar o revertir esas salidas.");

                var series = await db.ProductoSerie
                    .Where(s => s.id_detalle_entrada.HasValue && detalleIds.Contains(s.id_detalle_entrada.Value))
                    .ToListAsync();

                if (series.Any(s => s.id_salida != null))
                    throw new InvalidOperationException("No se puede eliminar esta entrada porque contiene series asociadas a una salida. Primero debe eliminar o revertir la salida que utiliza esas series.");

                if (series.Count > 0)
                    db.ProductoSerie.RemoveRange(series);

                db.DetalleEntrada.RemoveRange(entrada.DetalleEntrada);
            }

            db.Entrada.Remove(entrada);
            await db.SaveChangesAsync();
            await _audit.RegistrarAsync(entrada.IdUsuario, AuditAcciones.Eliminar, AuditEntidades.Entrada, (int)id, null);
            return true;
        }
    }
}
