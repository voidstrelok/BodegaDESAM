using System.ComponentModel.DataAnnotations;

namespace BodegaDESAM
{
    public enum TipoAjuste
    {
        Aumento,
        Disminucion
    }

    public partial class AjusteInventario
    {
        public int Id { get; set; }

        public string IdUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar una bodega")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una bodega")]
        public int id_bodega { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        public DateOnly Fecha { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

        [Required(ErrorMessage = "El motivo es obligatorio")]
        [StringLength(300, ErrorMessage = "El motivo debe tener máximo 300 caracteres")]
        public string Motivo { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "La observación debe tener máximo 200 caracteres")]
        public string Observacion { get; set; } = string.Empty;

        public virtual Bodega Bodega { get; set; } = null!;

        public virtual ICollection<DetalleAjuste> DetalleAjuste { get; set; } = new HashSet<DetalleAjuste>();
    }
}
