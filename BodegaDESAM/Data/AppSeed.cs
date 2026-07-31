using Microsoft.EntityFrameworkCore;

namespace BodegaDESAM.Data;

public static class AppSeed
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<PostgresDataContext>>();
        await using var db = await factory.CreateDbContextAsync();

        // Idempotente: si ya hay datos básicos, no volver a insertar
        if (await db.Marca.AnyAsync() || await db.CategoriaProducto.AnyAsync()
            || await db.Proveedor.AnyAsync() || await db.Bodega.AnyAsync() || await db.Establecimiento.AnyAsync())
            return;

        // ============ MARCAS ============
        var marcas = new[]
        {
            new Marca { Nombre = "Genérica" },
            new Marca { Nombre = "3M" },
            new Marca { Nombre = "Kimberly-Clark" },
        };
        db.Marca.AddRange(marcas);
        await db.SaveChangesAsync();



        // ============ PROVEEDORES ============
        var proveedores = new[]
        {
            new Proveedor 
            { 
                Nombre = "Distribuidora Medical Sur",
                RUT = "76.123.456-7",
                Direccion = "Av. Providencia 1234, Santiago",
                Telefono = "+56 2 2345 6789",
                Email = "ventas@medicalsur.cl",
                PersonaContacto = "Juan Pérez",
                Activo = true
            },
            new Proveedor 
            { 
                Nombre = "Insumos Hospitalarios Norte",
                RUT = "77.987.654-3",
                Direccion = "Los Carrera 567, La Serena",
                Telefono = "+56 51 234 5678",
                Email = "contacto@inhospnorte.cl",
                PersonaContacto = "María González",
                Activo = true
            },
            new Proveedor 
            { 
                Nombre = "Protección Industrial Ltda",
                RUT = "78.456.789-0",
                Direccion = "Av. España 890, Valparaíso",
                Telefono = "+56 32 456 7890",
                Email = "ventas@protecind.cl",
                PersonaContacto = "Carlos Rodríguez",
                Activo = true
            }
        };
        db.Proveedor.AddRange(proveedores);
        await db.SaveChangesAsync();

        // ============ CATEGORÍAS ============
        var categorias = new[]
        {
            new CategoriaProducto { Nombre = "Higiene y Desinfección" },
        };
        db.CategoriaProducto.AddRange(categorias);
        await db.SaveChangesAsync();

       
        // ============ BODEGA CENTRAL (única) ============
        var bodegaCentral = new Bodega
        {
            Codigo = "BOD-CENTRAL",
            Nombre = "Bodega Central DESAM",
            Direccion = "Avenida Ferroviaria, Monte Patria, Región de Coquimbo",
            EsPrincipal = true,
            Activa = true
        };
        db.Bodega.Add(bodegaCentral);
        await db.SaveChangesAsync();

        var bodegas = new[] { bodegaCentral };

        /* ============ UBICACIONES ============
        var ubicaciones = new[]
        {
            new Ubicacion { id_bodega = bodegaCentral.Id, Pasillo = "A", Estante = "01", Nivel = "1", Posicion = "A", CodigoCompleto = "A-01-1-A", CapacidadMaxima = 100 },
            new Ubicacion { id_bodega = bodegaCentral.Id, Pasillo = "A", Estante = "01", Nivel = "2", Posicion = "B", CodigoCompleto = "A-01-2-B", CapacidadMaxima = 100 },
            new Ubicacion { id_bodega = bodegaCentral.Id, Pasillo = "A", Estante = "02", Nivel = "1", Posicion = "A", CodigoCompleto = "A-02-1-A", CapacidadMaxima = 150 },
            new Ubicacion { id_bodega = bodegaCentral.Id, Pasillo = "B", Estante = "01", Nivel = "1", Posicion = "A", CodigoCompleto = "B-01-1-A", CapacidadMaxima = 200 },
            new Ubicacion { id_bodega = bodegaCentral.Id, Pasillo = "B", Estante = "02", Nivel = "3", Posicion = "C", CodigoCompleto = "B-02-3-C", CapacidadMaxima = 80 },
        };
        db.Ubicacion.AddRange(ubicaciones);
        await db.SaveChangesAsync();
*/
       
        // ============ ESTABLECIMIENTOS ============
        var establecimientos = new[]
        {
            new Establecimiento { Nombre = "Centro de Salud Familiar Monte Patria" },
            new Establecimiento { Nombre = "Centro de Salud Familiar Carén" },
            new Establecimiento { Nombre = "Centro de Salud Familiar El Palqui" },
            new Establecimiento { Nombre = "Centro de Salud Familiar Chañaral Alto" },
        };
        db.Establecimiento.AddRange(establecimientos);
        await db.SaveChangesAsync();
        
    }
}
