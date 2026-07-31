using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BodegaDESAM.Services
{
    public class AuditService
    {
        private readonly IDbContextFactory<PostgresDataContext> _factory;

        public AuditService(IDbContextFactory<PostgresDataContext> factory)
        {
            _factory = factory;
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
            string? ipOrigen = null)
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
    }

    public static class AuditEntidades
    {
        public const string Entrada = "Entrada";
        public const string Salida = "Salida";
        public const string AjusteInventario = "AjusteInventario";
    }
}
