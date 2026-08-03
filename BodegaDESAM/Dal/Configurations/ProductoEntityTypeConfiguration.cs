using BodegaDESAM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;

namespace BodegaDESAM
{
    public class ProductoEntityTypeConfiguration : IEntityTypeConfiguration<Producto>
    {
        public void Configure(EntityTypeBuilder<Producto> builder)
        {
            builder
                .HasKey(x => x.Id);

            builder
                .HasOne(x => x.CategoriaProducto)
                .WithMany(x => x.Productos)
                .HasForeignKey(x => x.id_categoria_producto)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id")
                .HasPrecision(32, 0);

            builder
                .Property(x => x.Nombre)
                .HasColumnName("nombre")
                .HasColumnType("character varying")
                .HasMaxLength(50)
                .IsRequired();

            builder
                .Property(x => x.id_categoria_producto)
                .HasColumnName("id_categoria_producto")
                .HasPrecision(32, 0);

            builder
                .ToTable("producto", "BodegaDESAM");
        }
    }
}
