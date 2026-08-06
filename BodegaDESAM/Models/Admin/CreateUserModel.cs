using System.ComponentModel.DataAnnotations;

namespace BodegaDESAM.Models.Admin;

public sealed class CreateUserVm
{
    [Required, EmailAddress]
    public string? Email { get; set; }

    [Required, MinLength(6)]
    public string? Password { get; set; }

    public string? Role { get; set; }

    [Required, MaxLength(100)]
    public string? Nombre { get; set; }

    [Required, MaxLength(100)]
    public string? Apellido { get; set; }

    [Required, MaxLength(100)]
    public string? ApellidoMaterno { get; set; }
}
