using BodegaDESAM;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

namespace BodegaDESAM
{
    public partial class PostgresDataContext : IdentityDbContext<IdentityUser>
    {
        public PostgresDataContext(DbContextOptions<PostgresDataContext> options) : base(options)
        {
        }

        public virtual DbSet<Producto> Producto { get; set; }
        public virtual DbSet<ProductoSerie> ProductoSerie { get; set; }
        public virtual DbSet<CategoriaProducto> CategoriaProducto { get; set; }
        public virtual DbSet<Marca> Marca { get; set; }
        public virtual DbSet<Modelo> Modelo { get; set; }
        public virtual DbSet<Entrada> Entrada { get; set; }
        public virtual DbSet<DetalleEntrada> DetalleEntrada { get; set; }
        public virtual DbSet<Proveedor> Proveedor { get; set; }
        public virtual DbSet<Salida> Salida { get; set; }
        public virtual DbSet<DetalleSalida> DetalleSalida { get; set; }
        public virtual DbSet<Establecimiento> Establecimiento { get; set; }
        public virtual DbSet<Bodega> Bodega { get; set; }
        public virtual DbSet<Ubicacion> Ubicacion { get; set; }
        public virtual DbSet<Lote> Lote { get; set; }
        public virtual DbSet<AuditLog> AuditLog { get; set; }
        public virtual DbSet<AjusteInventario> AjusteInventario { get; set; }
        public virtual DbSet<DetalleAjuste> DetalleAjuste { get; set; }
        public virtual DbSet<UsuarioBodega> UsuarioBodega { get; set; }
        public virtual DbSet<AlertaStock> AlertaStock { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
#if DEBUG
            optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();
#endif
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("BodegaDESAM");

            modelBuilder.ApplyConfiguration(new ProveedorEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new EntradaEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new DetalleEntradaEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ProductoEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ProductoSerieEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CategoriaProductoEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new MarcaEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ModeloEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new DetalleSalidaEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new SalidaEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new EstablecimientoEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new BodegaEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new UbicacionEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new LoteEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new UsuarioBodegaEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new AlertaStockEntityTypeConfiguration());

            modelBuilder.Entity<AuditLog>(e =>
            {
                e.ToTable("AuditLog");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).UseIdentityByDefaultColumn();
                e.Property(x => x.FechaHora).HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
                e.HasIndex(x => x.FechaHora);
                e.HasIndex(x => x.UsuarioId);
                e.HasIndex(x => new { x.Entidad, x.Accion });
                e.HasIndex(x => x.BodegaId);
                e.HasOne(x => x.Bodega).WithMany().HasForeignKey(x => x.BodegaId).OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<AjusteInventario>(e =>
            {
                e.ToTable("ajuste_inventario", "BodegaDESAM");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).UseIdentityByDefaultColumn();
                e.Property(x => x.Fecha).HasColumnType("date").HasConversion(typeof(DateOnlyValueConverter), typeof(DateOnlyValueComparer));
                e.HasOne(x => x.Bodega).WithMany().HasForeignKey(x => x.id_bodega).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<DetalleAjuste>(e =>
            {
                e.ToTable("detalle_ajuste", "BodegaDESAM");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).UseIdentityByDefaultColumn();
                e.Property(x => x.TipoAjuste).HasConversion<string>();
                e.HasOne(x => x.Ajuste).WithMany(x => x.DetalleAjuste).HasForeignKey(x => x.id_ajuste).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.Producto).WithMany().HasForeignKey(x => x.id_producto).OnDelete(DeleteBehavior.NoAction);
                e.HasOne(x => x.Marca).WithMany().HasForeignKey(x => x.id_marca).OnDelete(DeleteBehavior.NoAction);
                e.HasOne(x => x.Modelo).WithMany().HasForeignKey(x => x.id_modelo).OnDelete(DeleteBehavior.NoAction);
                e.HasOne(x => x.Lote).WithMany().HasForeignKey(x => x.id_lote).OnDelete(DeleteBehavior.NoAction);
                e.HasOne(x => x.DetalleEntradaOrigen).WithMany().HasForeignKey(x => x.id_detalle_entrada_origen).OnDelete(DeleteBehavior.NoAction);
                e.HasOne(x => x.DetalleAjusteOrigen).WithMany(x => x.DisminucionesOrigen).HasForeignKey(x => x.id_detalle_ajuste_origen).OnDelete(DeleteBehavior.NoAction);
                e.Property(x => x.FechaVencimiento).HasColumnType("date").HasConversion(typeof(DateOnlyValueConverter), typeof(DateOnlyValueComparer));
                e.Property(x => x.ValorUnitario)
                    .HasColumnName("valor_unitario")
                    .HasColumnType("bigint");
            });
        }
    }
}
