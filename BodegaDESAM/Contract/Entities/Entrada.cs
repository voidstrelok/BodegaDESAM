using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BodegaDESAM
{
    public partial class Entrada
    {
        public int Id { get; set; }

        public string IdUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar una bodega")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una bodega")]
        public int id_bodega { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un proveedor")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un proveedor")]
        public int id_proveedor { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        public DateOnly Fecha { get; set; }

        public virtual Bodega Bodega { get; set; } = null!;

        public virtual Proveedor Proveedor { get; set; } = null!;

        [Required(ErrorMessage = "El N° de documento es obligatorio")]
        [StringLength(50, ErrorMessage = "El N° de documento debe tener máximo 50 caracteres")]
        public string NDocumento { get; set; } = String.Empty;

        [StringLength(200, ErrorMessage = "La observación debe tener máximo 200 caracteres")]
        public string Observacion { get; set; } = String.Empty;

        public virtual ICollection<DetalleEntrada> DetalleEntrada { get; set; } = new HashSet<DetalleEntrada>();
    }
}
