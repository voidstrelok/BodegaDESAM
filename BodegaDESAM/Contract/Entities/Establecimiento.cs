using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BodegaDESAM
{
    public partial class Establecimiento
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(200, ErrorMessage = "El nombre debe tener máximo 200 caracteres")]
        public string Nombre { get; set; } = String.Empty;

        public virtual ICollection<Salida> Salida { get; set; } = new HashSet<Salida>();
    }
}
