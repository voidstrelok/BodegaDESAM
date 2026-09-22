using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BodegaDESAM;

public sealed class UsuarioBodegaEntityTypeConfiguration : IEntityTypeConfiguration<UsuarioBodega>
{
    public void Configure(EntityTypeBuilder<UsuarioBodega> builder)
    {
        builder.ToTable("usuario_bodega", "BodegaDESAM");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").UseIdentityByDefaultColumn();
        builder.Property(x => x.IdUsuario).HasColumnName("id_usuario").IsRequired();
        builder.Property(x => x.IdBodega).HasColumnName("id_bodega").IsRequired();
        builder.HasIndex(x => new { x.IdUsuario, x.IdBodega }).IsUnique();
        builder.HasIndex(x => x.IdBodega);
        builder.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.IdUsuario).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Bodega).WithMany().HasForeignKey(x => x.IdBodega).OnDelete(DeleteBehavior.Cascade);
    }
}
