using BodegaDESAM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;

namespace BodegaDESAM
{
    public class DetalleEntradaEntityTypeConfiguration : IEntityTypeConfiguration<DetalleEntrada>
    {
        public void Configure(EntityTypeBuilder<DetalleEntrada> builder)
        {
            builder
                .HasKey(x => x.Id);

            builder
                .HasOne(x => x.Producto)
                .WithMany(x => x.DetalleEntrada)
                .HasForeignKey(x => x.id_producto)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .HasOne(x => x.Entrada)
                .WithMany(x => x.DetalleEntrada)
                .HasForeignKey(x => x.id_entrada)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .HasOne(x => x.Marca)
                .WithMany(x => x.DetalleEntradas)
                .HasForeignKey(x => x.id_marca)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .HasOne(x => x.Modelo)
                .WithMany(x => x.DetalleEntradas)
                .HasForeignKey(x => x.id_modelo)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .HasOne(x => x.Lote)
                .WithMany(x => x.DetalleEntradas)
                .HasForeignKey(x => x.id_lote)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .HasOne(x => x.Ubicacion)
                .WithMany(x => x.DetalleEntradas)
                .HasForeignKey(x => x.id_ubicacion)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id")
                .HasPrecision(32, 0);

            builder
                .Property(x => x.id_entrada)
                .HasColumnName("id_entrada")
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
                .Property(x => x.FechaVencimiento)
                .HasColumnName("fecha_vencimiento")
                .HasColumnType("date")
                .HasConversion(typeof(DateOnlyValueConverter), typeof(DateOnlyValueComparer));

            builder
                .ToTable("detalle_entrada", "Bodega_dev");
        }
    }
}
