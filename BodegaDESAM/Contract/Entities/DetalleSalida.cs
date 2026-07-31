using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BodegaDESAM
{
    public partial class DetalleSalida
    {
        public int Id { get; set; }

        public int id_salida { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un producto")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un producto")]
        public int id_producto { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una marca")]
        [Range(1, long.MaxValue, ErrorMessage = "Debe seleccionar una marca")]
        public long id_marca { get; set; }

        public long? id_modelo { get; set; }

        public long? id_lote { get; set; }

        public long? id_ubicacion { get; set; }

        // Línea de recepción que abastece esta salida. Es nullable para conservar
        // documentos históricos creados antes de incorporar la trazabilidad.
        public int? id_detalle_entrada { get; set; }

        // Alternativa a la línea de entrada: un aumento de ajuste.
        public int? id_detalle_ajuste_origen { get; set; }

        public virtual Salida Salida { get; set; } = null!;

        public virtual Producto Producto { get; set; } = null!;

        public virtual Marca Marca { get; set; } = null!;

        public virtual Modelo? Modelo { get; set; }

        public virtual Lote? Lote { get; set; }

        public virtual Ubicacion? Ubicacion { get; set; }

        public virtual DetalleEntrada? DetalleEntrada { get; set; }
        public virtual DetalleAjuste? DetalleAjusteOrigen { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Cantidad { get; set; }
    }
}
