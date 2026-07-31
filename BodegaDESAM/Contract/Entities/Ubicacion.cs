using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BodegaDESAM
{
    /// <summary>
    /// Representa una ubicación física dentro de una bodega (pasillo-estante-nivel-posición)
    /// </summary>
    public partial class Ubicacion
    {
        public long Id { get; set; }

        public int id_bodega { get; set; }

        [Required(ErrorMessage = "El pasillo es obligatorio")]
        [StringLength(10, ErrorMessage = "El pasillo debe tener máximo 10 caracteres")]
        public string Pasillo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El estante es obligatorio")]
        [StringLength(10, ErrorMessage = "El estante debe tener máximo 10 caracteres")]
        public string Estante { get; set; } = string.Empty;

        [StringLength(10, ErrorMessage = "El nivel debe tener máximo 10 caracteres")]
        public string? Nivel { get; set; }

        [StringLength(10, ErrorMessage = "La posición debe tener máximo 10 caracteres")]
        public string? Posicion { get; set; }

        /// <summary>
        /// Código completo calculado (ej: A-03-2-B)
        /// </summary>
        [StringLength(50)]
        public string CodigoCompleto { get; set; } = string.Empty;

        public decimal? CapacidadMaxima { get; set; }

        public bool Bloqueada { get; set; } = false;

        [StringLength(200)]
        public string? MotivoBloqueo { get; set; }

        public virtual Bodega Bodega { get; set; } = null!;

        public virtual ICollection<DetalleEntrada> DetalleEntradas { get; set; } = new HashSet<DetalleEntrada>();
        public virtual ICollection<DetalleSalida> DetalleSalidas { get; set; } = new HashSet<DetalleSalida>();
    }
}
