using BodegaDESAM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;

namespace BodegaDESAM
{
    public class DetalleSalidaEntityTypeConfiguration : IEntityTypeConfiguration<DetalleSalida>
    {
        public void Configure(EntityTypeBuilder<DetalleSalida> builder)
        {
            builder
                .HasKey(x => x.Id);

            builder
                .HasOne(x => x.Salida)
                .WithMany(x => x.DetalleSalida)
                .HasForeignKey(x => x.id_salida)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .HasOne(x => x.Producto)
                .WithMany(x => x.DetalleSalida)
                .HasForeignKey(x => x.id_producto)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .HasOne(x => x.Marca)
                .WithMany()
                .HasForeignKey(x => x.id_marca)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .HasOne(x => x.Modelo)
                .WithMany()
                .HasForeignKey(x => x.id_modelo)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .HasOne(x => x.Lote)
                .WithMany(x => x.DetalleSalidas)
                .HasForeignKey(x => x.id_lote)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .HasOne(x => x.Ubicacion)
                .WithMany(x => x.DetalleSalidas)
                .HasForeignKey(x => x.id_ubicacion)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .HasOne(x => x.DetalleEntrada)
                .WithMany(x => x.DetalleSalidas)
                .HasForeignKey(x => x.id_detalle_entrada)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .HasOne(x => x.DetalleAjusteOrigen)
                .WithMany()
                .HasForeignKey(x => x.id_detalle_ajuste_origen)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id")
                .HasPrecision(32, 0);

            builder
                .Property(x => x.id_salida)
                .HasColumnName("id_salida")
                .HasPrecision(32, 0);

            builder
                .Property(x => x.id_producto)
                .HasColumnName("id_producto")
                .HasPrecision(32, 0);

            builder
                .Property(x => x.id_marca)
                .HasColumnName("id_marca")
                .HasPrecision(32, 0);

            builder
                .Property(x => x.Cantidad)
                .HasColumnName("cantidad")
                .HasPrecision(32, 0);

            builder
                .Property(x => x.id_modelo)
                .HasColumnName("id_modelo")
                .HasPrecision(32, 0);

            builder
                .Property(x => x.id_lote)
                .HasColumnName("id_lote")
                .HasPrecision(32, 0);

            builder
                .Property(x => x.id_ubicacion)
                .HasColumnName("id_ubicacion")
                .HasPrecision(32, 0);

            builder
                .Property(x => x.id_detalle_entrada)
                .HasColumnName("id_detalle_entrada")
                .HasPrecision(32, 0);

            builder.Property(x => x.id_detalle_ajuste_origen)
                .HasColumnName("id_detalle_ajuste_origen")
                .HasPrecision(32, 0);

            builder
                .ToTable("detalle_salida", "Bodega_dev");
        }
    }
}
