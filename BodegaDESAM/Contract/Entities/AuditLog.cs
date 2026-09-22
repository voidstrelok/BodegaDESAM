using System.ComponentModel.DataAnnotations;

namespace BodegaDESAM
{
    /// <summary>
    /// Registro de auditoría de operaciones sobre entidades críticas del sistema.
    /// </summary>
    public class AuditLog
    {
        public long Id { get; set; }

        [Required]
        [StringLength(450)]
        public string UsuarioId { get; set; } = string.Empty;

        /// <summary>Crear | Editar | Eliminar</summary>
        [Required]
        [StringLength(20)]
        public string Accion { get; set; } = string.Empty;

        /// <summary>Nombre de la entidad afectada: Entrada, Salida, AjusteInventario, etc.</summary>
        [Required]
        [StringLength(50)]
        public string Entidad { get; set; } = string.Empty;

        public int EntidadId { get; set; }
        public int? BodegaId { get; set; }

        /// <summary>Resumen JSON con los datos relevantes de la operación.</summary>
        public string? Detalle { get; set; }

        public DateTime FechaHora { get; set; } = DateTime.UtcNow;

        [StringLength(45)]
        public string? IpOrigen { get; set; }

        public Bodega? Bodega { get; set; }
    }
}
