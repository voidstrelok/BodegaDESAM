using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BodegaDESAM
{
    /// <summary>
    /// Representa una bodega o almacén físico
    /// </summary>
    public partial class Bodega
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El código es obligatorio")]
        [StringLength(20, ErrorMessage = "El código debe tener máximo 20 caracteres")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre debe tener máximo 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "La dirección debe tener máximo 200 caracteres")]
        public string? Direccion { get; set; }

        public bool EsPrincipal { get; set; }

        public bool Activa { get; set; } = true;

        public virtual ICollection<Ubicacion> Ubicaciones { get; set; } = new HashSet<Ubicacion>();
        public virtual ICollection<Entrada> Entradas { get; set; } = new HashSet<Entrada>();
        public virtual ICollection<Salida> Salidas { get; set; } = new HashSet<Salida>();
    }
}
