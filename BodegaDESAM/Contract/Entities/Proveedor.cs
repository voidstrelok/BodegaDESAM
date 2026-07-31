using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BodegaDESAM.Contract.Validation;

namespace BodegaDESAM
{
    public partial class Proveedor
    {
        public int Id { get; set; }

        [RutChileno]
        [StringLength(12, ErrorMessage = "El RUT debe tener el formato 11.111.111-1")]
        public string? RUT { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(200, ErrorMessage = "El nombre debe tener máximo 200 caracteres")]
        public string Nombre { get; set; } = String.Empty;

        [StringLength(200, ErrorMessage = "La dirección debe tener máximo 200 caracteres")]
        public string? Direccion { get; set; }

        [StringLength(50, ErrorMessage = "El teléfono debe tener máximo 50 caracteres")]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "El email no es válido")]
        [StringLength(100, ErrorMessage = "El email debe tener máximo 100 caracteres")]
        public string? Email { get; set; }

        [StringLength(100, ErrorMessage = "El nombre de contacto debe tener máximo 100 caracteres")]
        public string? PersonaContacto { get; set; }

        public bool Activo { get; set; } = true;

        public virtual ICollection<Entrada> Entrada { get; set; } = new HashSet<Entrada>();
    }
}
