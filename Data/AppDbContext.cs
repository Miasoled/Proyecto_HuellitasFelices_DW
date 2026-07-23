using HuellitasFelices.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HuellitasFelices.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Tablas de la base de datos
        public DbSet<Dueno> Duenos { get; set; }
        public DbSet<Mascota> Mascotas { get; set; }
        public DbSet<Consulta> Consultas { get; set; }
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<AnimalAdopcion> AnimalesAdopcion { get; set; }
        public DbSet<SolicitudAdopcion> SolicitudesAdopcion { get; set; }
        public DbSet<Tratamiento> Tratamientos { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Sucursal> Sucursales { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Inventario> Inventarios { get; set; }
        public DbSet<MovimientoInventario> MovimientosInventario { get; set; }
        public DbSet<Compra> Compras { get; set; }
        public DbSet<DetalleCompra> DetallesCompra { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Filtros globales de eliminación lógica ──────────────────────
            modelBuilder.Entity<Dueno>().HasQueryFilter(e => e.Activo);
            modelBuilder.Entity<Mascota>().HasQueryFilter(e => e.Activo);
            modelBuilder.Entity<Consulta>().HasQueryFilter(e => e.Activo);
            modelBuilder.Entity<Tratamiento>().HasQueryFilter(e => e.Activo);
            modelBuilder.Entity<Empleado>().HasQueryFilter(e => e.Activo);
            modelBuilder.Entity<AnimalAdopcion>().HasQueryFilter(e => e.Activo);
            modelBuilder.Entity<SolicitudAdopcion>().HasQueryFilter(e => e.Activo);
            modelBuilder.Entity<Categoria>().HasQueryFilter(e => e.Activo);
            modelBuilder.Entity<Proveedor>().HasQueryFilter(e => e.Activo);
            modelBuilder.Entity<Sucursal>().HasQueryFilter(e => e.Activo);
            // Producto and Compra intentionally lack query filters — they are
            // required ends of relationships (Inventario, Movimiento, DetalleCompra)
            // and EF warns when the principal is filtered but the dependent is not.

            // ── Índices para búsquedas frecuentes ──────────────────────────
            modelBuilder.Entity<Dueno>().HasIndex(d => d.Email);
            modelBuilder.Entity<Dueno>().HasIndex(d => d.Nombre);
            modelBuilder.Entity<Mascota>().HasIndex(m => m.Nombre);
            modelBuilder.Entity<Mascota>().HasIndex(m => m.DuenoId);
            modelBuilder.Entity<Consulta>().HasIndex(c => c.Estado);
            modelBuilder.Entity<Consulta>().HasIndex(c => c.MascotaId);
            modelBuilder.Entity<Consulta>().HasIndex(c => c.FechaConsulta);
            modelBuilder.Entity<Consulta>().HasIndex(c => c.VeterinarioId);
            modelBuilder.Entity<Empleado>().HasIndex(e => e.Cargo);
            modelBuilder.Entity<Empleado>().HasIndex(e => e.Email);
            modelBuilder.Entity<Tratamiento>().HasIndex(t => t.ConsultaId);
            modelBuilder.Entity<SolicitudAdopcion>().HasIndex(s => s.Estado);
            modelBuilder.Entity<AuditLog>().HasIndex(a => a.FechaCreacion);
            modelBuilder.Entity<AuditLog>().HasIndex(a => a.Accion);
            modelBuilder.Entity<AuditLog>().HasIndex(a => a.Entidad);
            modelBuilder.Entity<AuditLog>().HasIndex(a => a.UsuarioEmail);
            modelBuilder.Entity<Producto>().HasIndex(p => p.Nombre);
            modelBuilder.Entity<Producto>().HasIndex(p => p.CodigoBarras);
            modelBuilder.Entity<Producto>().HasIndex(p => p.CategoriaId);
            modelBuilder.Entity<Inventario>().HasIndex(i => new { i.ProductoId, i.SucursalId }).IsUnique();
            modelBuilder.Entity<MovimientoInventario>().HasIndex(m => m.FechaMovimiento);
            modelBuilder.Entity<MovimientoInventario>().HasIndex(m => m.ProductoId);
            modelBuilder.Entity<MovimientoInventario>().HasIndex(m => m.TipoMovimiento);

            // ── Mapeo de tablas ────────────────────────────────────────────
            modelBuilder.Entity<Tratamiento>().ToTable("Tratamientos");
            modelBuilder.Entity<AuditLog>().ToTable("AuditLogs");
            modelBuilder.Entity<Categoria>().ToTable("Categorias");
            modelBuilder.Entity<Proveedor>().ToTable("Proveedores");
            modelBuilder.Entity<Sucursal>().ToTable("Sucursales");
            modelBuilder.Entity<Producto>().ToTable("Productos");
            modelBuilder.Entity<Inventario>().ToTable("Inventarios");
            modelBuilder.Entity<MovimientoInventario>().ToTable("MovimientosInventario");
            modelBuilder.Entity<Compra>().ToTable("Compras");
            modelBuilder.Entity<DetalleCompra>().ToTable("DetallesCompra");

            // ── Relaciones ─────────────────────────────────────────────────

            // Mascota -> Dueno
            modelBuilder.Entity<Mascota>()
                .HasOne(m => m.Dueno)
                .WithMany(d => d.Mascotas)
                .HasForeignKey(m => m.DuenoId)
                .OnDelete(DeleteBehavior.SetNull);

            // Consulta -> Mascota
            modelBuilder.Entity<Consulta>()
                .HasOne(c => c.Mascota)
                .WithMany(m => m.Consultas)
                .HasForeignKey(c => c.MascotaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Consulta -> Veterinario (Empleado opcional)
            modelBuilder.Entity<Consulta>()
                .HasOne(c => c.Veterinario)
                .WithMany()
                .HasForeignKey(c => c.VeterinarioId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // SolicitudAdopcion -> AnimalAdopcion
            modelBuilder.Entity<SolicitudAdopcion>()
                .HasOne(s => s.AnimalAdopcion)
                .WithMany(a => a.Solicitudes)
                .HasForeignKey(s => s.AnimalAdopcionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Tratamiento -> Consulta
            modelBuilder.Entity<Tratamiento>()
                .HasOne(t => t.Consulta)
                .WithMany(c => c.Tratamientos)
                .HasForeignKey(t => t.ConsultaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Producto -> Categoria
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Producto -> Proveedor
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Proveedor)
                .WithMany(pr => pr.Productos)
                .HasForeignKey(p => p.ProveedorId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // Inventario -> Producto + Sucursal
            modelBuilder.Entity<Inventario>()
                .HasOne(i => i.Producto)
                .WithMany(p => p.Inventarios)
                .HasForeignKey(i => i.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Inventario>()
                .HasOne(i => i.Sucursal)
                .WithMany(s => s.Inventarios)
                .HasForeignKey(i => i.SucursalId)
                .OnDelete(DeleteBehavior.Cascade);

            // MovimientoInventario
            modelBuilder.Entity<MovimientoInventario>()
                .HasOne(m => m.Producto)
                .WithMany(p => p.Movimientos)
                .HasForeignKey(m => m.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MovimientoInventario>()
                .HasOne(m => m.SucursalOrigen)
                .WithMany(s => s.MovimientosOrigen)
                .HasForeignKey(m => m.SucursalOrigenId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MovimientoInventario>()
                .HasOne(m => m.SucursalDestino)
                .WithMany(s => s.MovimientosDestino)
                .HasForeignKey(m => m.SucursalDestinoId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // Compra -> Proveedor + Sucursal
            modelBuilder.Entity<Compra>()
                .HasOne(c => c.Proveedor)
                .WithMany(p => p.Compras)
                .HasForeignKey(c => c.ProveedorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Compra>()
                .HasOne(c => c.Sucursal)
                .WithMany()
                .HasForeignKey(c => c.SucursalId)
                .OnDelete(DeleteBehavior.Restrict);

            // DetalleCompra -> Compra + Producto
            modelBuilder.Entity<DetalleCompra>()
                .HasOne(d => d.Compra)
                .WithMany(c => c.Detalles)
                .HasForeignKey(d => d.CompraId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DetalleCompra>()
                .HasOne(d => d.Producto)
                .WithMany(p => p.DetallesCompra)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
