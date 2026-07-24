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
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Inventario> Inventarios { get; set; }
        public DbSet<MovimientoInventario> MovimientosInventario { get; set; }
        public DbSet<Compra> Compras { get; set; }
        public DbSet<DetalleCompra> DetallesCompra { get; set; }
        public DbSet<ConsultaMedicamento> ConsultaMedicamentos { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetallesVenta { get; set; }
        public DbSet<EmailLog> EmailLogs { get; set; }
        public DbSet<Pago> Pagos { get; set; }

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
            modelBuilder.Entity<Venta>().HasQueryFilter(e => e.Activo);
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
            modelBuilder.Entity<Inventario>().HasIndex(i => i.ProductoId).IsUnique();
            modelBuilder.Entity<MovimientoInventario>().HasIndex(m => m.FechaMovimiento);
            modelBuilder.Entity<MovimientoInventario>().HasIndex(m => m.ProductoId);
            modelBuilder.Entity<MovimientoInventario>().HasIndex(m => m.TipoMovimiento);
            modelBuilder.Entity<ConsultaMedicamento>().HasIndex(cm => cm.ConsultaId);
            modelBuilder.Entity<ConsultaMedicamento>().HasIndex(cm => cm.ProductoId);
            modelBuilder.Entity<Venta>().HasIndex(v => v.ConsultaId);
            modelBuilder.Entity<Venta>().HasIndex(v => v.DuenoId);
            modelBuilder.Entity<Venta>().HasIndex(v => v.Estado);
            modelBuilder.Entity<DetalleVenta>().HasIndex(dv => dv.VentaId);
            modelBuilder.Entity<DetalleVenta>().HasIndex(dv => dv.ProductoId);
            modelBuilder.Entity<EmailLog>().HasIndex(e => e.Estado);
            modelBuilder.Entity<EmailLog>().HasIndex(e => e.TipoNotificacion);
            modelBuilder.Entity<EmailLog>().HasIndex(e => e.FechaSolicitud);

            // ── Pago ─────────────────────────────────────────────────────
            modelBuilder.Entity<Pago>().HasIndex(p => p.Estado);
            modelBuilder.Entity<Pago>().HasIndex(p => p.ProveedorPago);
            modelBuilder.Entity<Pago>().HasIndex(p => p.IdentificadorExterno);
            modelBuilder.Entity<Pago>().HasIndex(p => p.VentaId);
            modelBuilder.Entity<Pago>().HasIndex(p => p.NumeroPago);

            // ── Mapeo de tablas ────────────────────────────────────────────
            modelBuilder.Entity<Tratamiento>().ToTable("Tratamientos");
            modelBuilder.Entity<AuditLog>().ToTable("AuditLogs");
            modelBuilder.Entity<Categoria>().ToTable("Categorias");
            modelBuilder.Entity<Proveedor>().ToTable("Proveedores");
            modelBuilder.Entity<Producto>().ToTable("Productos");
            modelBuilder.Entity<Inventario>().ToTable("Inventarios");
            modelBuilder.Entity<MovimientoInventario>().ToTable("MovimientosInventario");
            modelBuilder.Entity<Compra>().ToTable("Compras");
            modelBuilder.Entity<DetalleCompra>().ToTable("DetallesCompra");
            modelBuilder.Entity<ConsultaMedicamento>().ToTable("ConsultaMedicamentos");
            modelBuilder.Entity<Venta>().ToTable("Ventas");
            modelBuilder.Entity<DetalleVenta>().ToTable("DetallesVenta");
            modelBuilder.Entity<EmailLog>().ToTable("EmailLogs");
            modelBuilder.Entity<Pago>().ToTable("Pagos");

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

            // Inventario -> Producto
            modelBuilder.Entity<Inventario>()
                .HasOne(i => i.Producto)
                .WithMany(p => p.Inventarios)
                .HasForeignKey(i => i.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);

            // MovimientoInventario
            modelBuilder.Entity<MovimientoInventario>()
                .HasOne(m => m.Producto)
                .WithMany(p => p.Movimientos)
                .HasForeignKey(m => m.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Compra -> Proveedor
            modelBuilder.Entity<Compra>()
                .HasOne(c => c.Proveedor)
                .WithMany(p => p.Compras)
                .HasForeignKey(c => c.ProveedorId)
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

            // ConsultaMedicamento -> Consulta + Producto
            modelBuilder.Entity<ConsultaMedicamento>()
                .HasOne(cm => cm.Consulta)
                .WithMany(c => c.Medicamentos)
                .HasForeignKey(cm => cm.ConsultaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ConsultaMedicamento>()
                .HasOne(cm => cm.Producto)
                .WithMany()
                .HasForeignKey(cm => cm.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Venta -> Consulta (1:1 optional — nullable for store purchases)
            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Consulta)
                .WithOne(c => c.Venta)
                .HasForeignKey<Venta>(v => v.ConsultaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Venta -> Dueno
            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Dueno)
                .WithMany()
                .HasForeignKey(v => v.DuenoId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // DetalleVenta -> Venta + Producto
            modelBuilder.Entity<DetalleVenta>()
                .HasOne(dv => dv.Venta)
                .WithMany(v => v.Detalles)
                .HasForeignKey(dv => dv.VentaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DetalleVenta>()
                .HasOne(dv => dv.Producto)
                .WithMany()
                .HasForeignKey(dv => dv.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Pago -> Venta
            modelBuilder.Entity<Pago>()
                .HasOne(p => p.Venta)
                .WithMany()
                .HasForeignKey(p => p.VentaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Pago -> Consulta (opcional)
            modelBuilder.Entity<Pago>()
                .HasOne(p => p.Consulta)
                .WithMany()
                .HasForeignKey(p => p.ConsultaId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // Pago -> Dueno
            modelBuilder.Entity<Pago>()
                .HasOne(p => p.Dueno)
                .WithMany()
                .HasForeignKey(p => p.DuenoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
