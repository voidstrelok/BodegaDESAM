using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BodegaDESAM;

public sealed class AlertaStockEntityTypeConfiguration : IEntityTypeConfiguration<AlertaStock>
{
    public void Configure(EntityTypeBuilder<AlertaStock> builder)
    {
        builder.ToTable("alerta_stock", "BodegaDESAM");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityByDefaultColumn();
        builder.Property(x => x.IdBodega).HasColumnName("id_bodega").IsRequired();
        builder.Property(x => x.IdProducto).HasColumnName("id_producto").IsRequired();
        builder.Property(x => x.StockMinimo).HasColumnName("stock_minimo").IsRequired();
        builder.HasIndex(x => new { x.IdBodega, x.IdProducto }).IsUnique();
        builder.HasOne(x => x.Bodega).WithMany().HasForeignKey(x => x.IdBodega).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Producto).WithMany().HasForeignKey(x => x.IdProducto).OnDelete(DeleteBehavior.Cascade);
    }
}
