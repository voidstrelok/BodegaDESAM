using Microsoft.AspNetCore.Identity;

namespace BodegaDESAM;

/// <summary>Asignación explícita de una bodega a un usuario no administrador.</summary>
public class UsuarioBodega
{
    public int Id { get; set; }
    public string IdUsuario { get; set; } = string.Empty;
    public int IdBodega { get; set; }
    public IdentityUser Usuario { get; set; } = null!;
    public Bodega Bodega { get; set; } = null!;
}
