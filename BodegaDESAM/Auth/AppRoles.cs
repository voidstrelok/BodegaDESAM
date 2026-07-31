namespace BodegaDESAM.Auth;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Bodega = "Bodega";
    public const string Lectura = "Lectura";

    public const string AdminOrBodega = Admin + "," + Bodega;
    public const string Public = Admin + "," + Bodega + "," + Lectura;
}
