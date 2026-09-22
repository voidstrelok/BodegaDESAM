using System.ComponentModel.DataAnnotations;

namespace BodegaDESAM;

/// <summary>Umbral de reposición de un producto dentro de una bodega.</summary>
public sealed class AlertaStock
{
    public int Id { get; set; }
    [Range(1, int.MaxValue)] public int IdBodega { get; set; }
    [Range(1, int.MaxValue)] public int IdProducto { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "El stock mínimo debe ser mayor que cero.")] public int StockMinimo { get; set; }
    public Bodega Bodega { get; set; } = null!;
    public Producto Producto { get; set; } = null!;
}
