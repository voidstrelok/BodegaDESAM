using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BodegaDESAM
{
    public class UbicacionEntityTypeConfiguration : IEntityTypeConfiguration<Ubicacion>
    {
        public void Configure(EntityTypeBuilder<Ubicacion> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id")
                .HasPrecision(32, 0);

            builder.HasOne(x => x.Bodega)
                .WithMany(x => x.Ubicaciones)
                .HasForeignKey(x => x.id_bodega)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Property(x => x.Pasillo)
                .HasColumnName("pasillo")
                .HasColumnType("character varying")
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(x => x.Estante)
                .HasColumnName("estante")
                .HasColumnType("character varying")
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(x => x.Nivel)
                .HasColumnName("nivel")
                .HasColumnType("character varying")
                .HasMaxLength(10);

            builder.Property(x => x.Posicion)
                .HasColumnName("posicion")
                .HasColumnType("character varying")
                .HasMaxLength(10);

            builder.Property(x => x.CodigoCompleto)
                .HasColumnName("codigo_completo")
                .HasColumnType("character varying")
                .HasMaxLength(50)
                .IsRequired();

            // Índice único por bodega + código completo
            builder.HasIndex(x => new { x.id_bodega, x.CodigoCompleto }).IsUnique();

            builder.Property(x => x.CapacidadMaxima)
                .HasColumnName("capacidad_maxima")
                .HasPrecision(18, 2);

            builder.Property(x => x.Bloqueada)
                .HasColumnName("bloqueada")
                .HasDefaultValue(false);

            builder.Property(x => x.MotivoBloqueo)
                .HasColumnName("motivo_bloqueo")
                .HasColumnType("character varying")
                .HasMaxLength(200);

            builder.ToTable("ubicacion", "BodegaDESAM");
        }
    }
}
