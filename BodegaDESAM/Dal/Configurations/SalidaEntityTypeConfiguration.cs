using BodegaDESAM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;

namespace BodegaDESAM
{
    public class SalidaEntityTypeConfiguration : IEntityTypeConfiguration<Salida>
    {
        public void Configure(EntityTypeBuilder<Salida> builder)
        {
            builder
                .HasKey(x => x.Id);

            builder
                .HasOne(x => x.EStablecimiento)
                .WithMany(x => x.Salida)
                .HasForeignKey(x => x.id_establecimiento)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .HasOne(x => x.Bodega)
                .WithMany(x => x.Salidas)
                .HasForeignKey(x => x.id_bodega)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id")
                .HasPrecision(32, 0);

            builder
                .Property(x => x.Observacion)
                .HasColumnName("observacion")
                .HasColumnType("character varying"); 
            
            builder
                .Property(x => x.Solicitante)
                .HasColumnName("solicitante")
                .HasColumnType("character varying");

            builder
                .Property(x => x.Fecha)
                .HasColumnName("fecha")
                .HasColumnType("date");

            builder
                .Property(x => x.IdUsuario)
                .IsRequired()
                .HasColumnName("id_usuario")
                .HasColumnType("text");

            builder
                .ToTable("salida", "Bodega_dev");
        }
    }
}
