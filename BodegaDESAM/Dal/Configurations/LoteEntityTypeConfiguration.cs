using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BodegaDESAM
{
    public class LoteEntityTypeConfiguration : IEntityTypeConfiguration<Lote>
    {
        public void Configure(EntityTypeBuilder<Lote> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id")
                .HasPrecision(32, 0);

            builder.HasOne(x => x.Producto)
                .WithMany()
                .HasForeignKey(x => x.id_producto)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Property(x => x.id_producto)
                .IsRequired()
                .HasColumnName("id_producto")
                .HasPrecision(32, 0);

            builder.Property(x => x.Codigo)
                .HasColumnName("codigo")
                .HasColumnType("character varying")
                .HasMaxLength(50)
                .IsRequired();

            builder.HasIndex(x => new { x.id_producto, x.Codigo }).IsUnique();

            builder.Property(x => x.FechaFabricacion)
                .HasColumnName("fecha_fabricacion")
                .HasColumnType("date")
                .HasConversion(typeof(DateOnlyValueConverter), typeof(DateOnlyValueComparer));

            builder.Property(x => x.FechaVencimiento)
                .HasColumnName("fecha_vencimiento")
                .HasColumnType("date")
                .HasConversion(typeof(DateOnlyValueConverter), typeof(DateOnlyValueComparer));

            builder.Property(x => x.Observaciones)
                .HasColumnName("observaciones")
                .HasColumnType("character varying")
                .HasMaxLength(200);

            builder.ToTable("lote", "Bodega_dev");
        }
    }
}
