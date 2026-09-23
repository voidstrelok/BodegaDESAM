namespace BodegaDESAM.Models.Admin;

public sealed class UserWithRolesVm
{
    public required string Id { get; init; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? Nombre { get; set; }
    public string? Apellido { get; set; }
    public string? ApellidoMaterno { get; set; }
    public string? NuevaClave { get; set; }
    public bool AccesoActivo { get; set; }
    public IList<string> Roles { get; init; } = new List<string>();

    public bool IsEditing { get; set; }
}
