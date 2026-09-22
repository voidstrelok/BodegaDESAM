using System.ComponentModel.DataAnnotations;

namespace BodegaDESAM
{
    public partial class DetalleAjuste
    {
        public int Id { get; set; }

        public int id_ajuste { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un producto")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un producto")]
        public int id_producto { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una marca")]
        [Range(1, long.MaxValue, ErrorMessage = "Debe seleccionar una marca")]
        public long id_marca { get; set; }

        public long? id_modelo { get; set; }

        public long? id_lote { get; set; }

        // Origen consumido cuando el ajuste es una disminución. Sólo uno de los
        // dos orígenes puede estar informado.
        public int? id_detalle_entrada_origen { get; set; }
        public int? id_detalle_ajuste_origen { get; set; }

        [Required(ErrorMessage = "El tipo de ajuste es obligatorio")]
        public TipoAjuste TipoAjuste { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, long.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public long Cantidad { get; set; }

        [Range(1, long.MaxValue, ErrorMessage = "El valor unitario debe ser mayor a 0")]
        public long? ValorUnitario { get; set; }

        public DateOnly? FechaVencimiento { get; set; }

        [StringLength(200)]
        public string? Observacion { get; set; }

        public virtual AjusteInventario Ajuste { get; set; } = null!;

        public virtual Producto Producto { get; set; } = null!;

        public virtual Marca Marca { get; set; } = null!;

        public virtual Modelo? Modelo { get; set; }
        public virtual Lote? Lote { get; set; }
        public virtual DetalleEntrada? DetalleEntradaOrigen { get; set; }
        public virtual DetalleAjuste? DetalleAjusteOrigen { get; set; }
        public virtual ICollection<DetalleAjuste> DisminucionesOrigen { get; set; } = new HashSet<DetalleAjuste>();
    }
}
