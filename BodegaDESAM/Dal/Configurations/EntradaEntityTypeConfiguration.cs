using BodegaDESAM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;

namespace BodegaDESAM
{
    public class EntradaEntityTypeConfiguration : IEntityTypeConfiguration<Entrada>
    {
        public void Configure(EntityTypeBuilder<Entrada> builder)
        {
            builder
                .HasKey(x => x.Id);

            builder
                .HasOne(x => x.Proveedor)
                .WithMany(x => x.Entrada)
                .HasForeignKey(x => x.id_proveedor)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .HasOne(x => x.Bodega)
                .WithMany(x => x.Entradas)
                .HasForeignKey(x => x.id_bodega)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id")
                .HasPrecision(32, 0);

            builder
                .Property(x => x.Fecha)
                .HasColumnName("fecha")
                .HasColumnType("date")
                .HasConversion(typeof(DateOnlyValueConverter), typeof(DateOnlyValueComparer));

            builder
                .Property(x => x.NDocumento)
                .HasColumnName("n_documento")
                .HasColumnType("character varying");

            builder
                .Property(x => x.Observacion)
                .HasColumnName("observacion")
                .HasColumnType("character varying");

            builder
                .Property(x => x.IdUsuario)
                .IsRequired()
                .HasColumnName("id_usuario")
                .HasColumnType("text");

            builder
                .ToTable("entrada", "Bodega_dev");
        }
    }
}
