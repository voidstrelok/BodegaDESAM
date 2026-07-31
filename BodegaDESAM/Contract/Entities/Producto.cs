using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BodegaDESAM
{
    public partial class Producto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una categoría")]
        [Range(1, long.MaxValue, ErrorMessage = "Debe seleccionar una categoría")]
        public long id_categoria_producto { get; set; }
        
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres",MinimumLength =2)]
        public string Nombre { get; set; } = String.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo debe ser 0 o mayor")]
        public int? StockMinimo { get; set; }

        public virtual CategoriaProducto CategoriaProducto { get; set; } = null!;

        public virtual ICollection<ProductoSerie> ProductoSeries { get; set; } = new HashSet<ProductoSerie>();

        public virtual ICollection<DetalleEntrada> DetalleEntrada { get; set; } = new HashSet<DetalleEntrada>();

        public virtual ICollection<DetalleSalida> DetalleSalida { get; set; } = new HashSet<DetalleSalida>();
    }
}
