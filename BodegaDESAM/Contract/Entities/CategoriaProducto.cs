using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BodegaDESAM
{
    public partial class CategoriaProducto
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres", MinimumLength = 2)]
        public string Nombre { get; set; } = string.Empty;

        public virtual ICollection<Producto> Productos { get; set; } = new HashSet<Producto>();
    }
}
