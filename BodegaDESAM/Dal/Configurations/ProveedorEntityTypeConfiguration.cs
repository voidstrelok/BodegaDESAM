using BodegaDESAM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;

namespace BodegaDESAM
{
    public class ProveedorEntityTypeConfiguration : IEntityTypeConfiguration<Proveedor>
    {
        public void Configure(EntityTypeBuilder<Proveedor> builder)
        {
            builder
                .HasKey(x => x.Id);

            builder
                .Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id")
                .HasPrecision(32, 0);

            builder
                .Property(x => x.Nombre)
                .IsRequired()
                .HasColumnName("nombre")
                .HasColumnType("character varying")
                .HasMaxLength(200);

            builder
                .Property(x => x.RUT)
                .HasColumnName("rut")
                .HasColumnType("character varying")
                .HasMaxLength(20);

            builder
                .Property(x => x.Direccion)
                .HasColumnName("direccion")
                .HasColumnType("character varying")
                .HasMaxLength(200);

            builder
                .Property(x => x.Telefono)
                .HasColumnName("telefono")
                .HasColumnType("character varying")
                .HasMaxLength(50);

            builder
                .Property(x => x.Email)
                .HasColumnName("email")
                .HasColumnType("character varying")
                .HasMaxLength(100);

            builder
                .Property(x => x.PersonaContacto)
                .HasColumnName("persona_contacto")
                .HasColumnType("character varying")
                .HasMaxLength(100);

            builder
                .Property(x => x.Activo)
                .IsRequired()
                .HasColumnName("activo")
                .HasDefaultValue(true);

            builder
                .ToTable("proveedor", "BodegaDESAM");
        }
    }
}
