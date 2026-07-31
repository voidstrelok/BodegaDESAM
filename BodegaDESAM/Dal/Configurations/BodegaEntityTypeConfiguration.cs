using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BodegaDESAM
{
    public class BodegaEntityTypeConfiguration : IEntityTypeConfiguration<Bodega>
    {
        public void Configure(EntityTypeBuilder<Bodega> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id")
                .HasPrecision(32, 0);

            builder.Property(x => x.Codigo)
                .HasColumnName("codigo")
                .HasColumnType("character varying")
                .HasMaxLength(20)
                .IsRequired();

            builder.HasIndex(x => x.Codigo).IsUnique();

            builder.Property(x => x.Nombre)
                .HasColumnName("nombre")
                .HasColumnType("character varying")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Direccion)
                .HasColumnName("direccion")
                .HasColumnType("character varying")
                .HasMaxLength(200);

            builder.Property(x => x.EsPrincipal)
                .HasColumnName("es_principal")
                .HasDefaultValue(false);

            builder.Property(x => x.Activa)
                .HasColumnName("activa")
                .HasDefaultValue(true);

            builder.ToTable("bodega", "Bodega_dev");
        }
    }
}
