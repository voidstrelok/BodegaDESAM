namespace BodegaDESAM.Services;

/// <summary>
/// Representa un SKU (Producto + Marca + Modelo + Lote) disponible en inventario
/// </summary>
public class ProductoSkuDisponible
{
    public int IdDetalleEntrada { get; set; }
    public int? IdDetalleAjuste { get; set; }
    public bool EsAjusteAumento => IdDetalleAjuste.HasValue;
    public int IdEntrada { get; set; }
    public string? NombreProveedor { get; set; }
    public string? NumeroDocumento { get; set; }
    public int IdProducto { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public long IdMarca { get; set; }
    public string NombreMarca { get; set; } = string.Empty;
    public long? IdModelo { get; set; }
    public string? NombreModelo { get; set; }
    public long? IdLote { get; set; }
    public string? CodigoLote { get; set; }
    public DateOnly? FechaVencimiento { get; set; }
    public long StockDisponible { get; set; }

    /// <summary>
    /// Identificador único del SKU para el selector (incluye lote si aplica)
    /// </summary>
    public string SkuKey => EsAjusteAumento ? $"A{IdDetalleAjuste}" : $"E{IdDetalleEntrada}";

    /// <summary>
    /// Texto descriptivo para mostrar en el selector
    /// </summary>
    public string DisplayText
    {
        get
        {
            var texto = $"{NombreProducto} - {NombreMarca}";
            if (!string.IsNullOrWhiteSpace(NombreModelo))
                texto += $" - {NombreModelo}";
            if (!string.IsNullOrWhiteSpace(CodigoLote))
            {
                texto += $" | Lote: {CodigoLote}";
                if (FechaVencimiento.HasValue)
                    texto += $" (Venc: {FechaVencimiento.Value:dd/MM/yyyy})";
            }
            if (EsAjusteAumento)
                texto += " | Origen: ajuste de aumento";
            else if (!string.IsNullOrWhiteSpace(NombreProveedor) || !string.IsNullOrWhiteSpace(NumeroDocumento))
                texto += $" | {NombreProveedor ?? "Sin proveedor"} - Doc.: {NumeroDocumento ?? "—"}";
            texto += $" (Stock: {StockDisponible})";
            return texto;
        }
    }
}
