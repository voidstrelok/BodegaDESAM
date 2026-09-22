using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BodegaDESAM.Services
{
    public class AuditService
    {
        private readonly IDbContextFactory<PostgresDataContext> _factory;
        private readonly AuditContextService _context;

        public AuditService(IDbContextFactory<PostgresDataContext> factory, AuditContextService context)
        {
            _factory = factory;
            _context = context;
        }

        /// <summary>
        /// Registra una operación de auditoría de forma asíncrona y no bloqueante.
        /// </summary>
        public async Task RegistrarAsync(
            string usuarioId,
            string accion,
            string entidad,
            int entidadId,
            object? detalle = null,
            string? ipOrigen = null,
            int? bodegaId = null)
        {
            try
            {
                using var db = _factory.CreateDbContext();
                db.AuditLog.Add(new AuditLog
                {
                    UsuarioId = usuarioId,
                    Accion = accion,
                    Entidad = entidad,
                    EntidadId = entidadId,
                    BodegaId = bodegaId,
                    Detalle = detalle is not null
                        ? JsonSerializer.Serialize(detalle, new JsonSerializerOptions { WriteIndented = false })
                        : null,
                    FechaHora = DateTime.UtcNow,
                    IpOrigen = ipOrigen
                });
                await db.SaveChangesAsync();
            }
            catch
            {
                // La auditoría nunca debe bloquear el flujo principal.
            }
        }

        public async Task RegistrarActualAsync(string accion, string entidad, int entidadId, object? detalle = null, int? bodegaId = null)
        {
            var actor = await _context.GetActorAsync();
            await RegistrarAsync(actor.UsuarioId, accion, entidad, entidadId, detalle, actor.Ip, bodegaId);
        }

        public async Task<List<AuditLog>> GetUltimosAsync(int cantidad = 50)
        {
            using var db = _factory.CreateDbContext();
            return await db.AuditLog
                .AsNoTracking()
                .OrderByDescending(a => a.FechaHora)
                .Take(cantidad)
                .ToListAsync();
        }
    }

    public static class AuditAcciones
    {
        public const string Crear = "Crear";
        public const string Editar = "Editar";
        public const string Eliminar = "Eliminar";
        public const string Asignar = "Asignar";
        public const string Quitar = "Quitar";
        public const string Activar = "Activar";
        public const string Desactivar = "Desactivar";
    }

    public static class AuditEntidades
    {
        public const string Entrada = "Entrada";
        public const string Salida = "Salida";
        public const string AjusteInventario = "AjusteInventario";
        public const string AlertaStock = "AlertaStock";
        public const string Bodega = "Bodega";
        public const string Usuario = "Usuario";
        public const string UsuarioBodega = "UsuarioBodega";
    }
}
