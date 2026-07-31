using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BodegaDESAM
{
    public class ModeloEntityTypeConfiguration : IEntityTypeConfiguration<Modelo>
    {
        public void Configure(EntityTypeBuilder<Modelo> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id")
                .HasPrecision(32, 0);

            builder.HasOne(x => x.Marca)
                .WithMany()
                .HasForeignKey(x => x.id_marca)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Property(x => x.id_marca)
                .IsRequired()
                .HasColumnName("id_marca")
                .HasPrecision(32, 0);

            builder.Property(x => x.Nombre)
                .HasColumnName("nombre")
                .HasColumnType("character varying")
                .HasMaxLength(100)
                .IsRequired();

            // Índice único compuesto: un modelo con el mismo nombre no puede repetirse dentro de una marca
            builder.HasIndex(x => new { x.id_marca, x.Nombre }).IsUnique();

            builder.ToTable("modelo", "Bodega_dev");
        }
    }
}
