using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BodegaDESAM
{
    public partial class Establecimiento
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = String.Empty;

        public virtual ICollection<Salida> Salida { get; set; } = new HashSet<Salida>();
    }
}
