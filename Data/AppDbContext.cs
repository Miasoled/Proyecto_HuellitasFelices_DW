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
        public DbSet<Pago> Pagos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Mapeo explícito de nombres de tablas
            modelBuilder.Entity<Tratamiento>().ToTable("Tratamientos");
            modelBuilder.Entity<Pago>().ToTable("Pagos");

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

            // Pago -> Dueno
            modelBuilder.Entity<Pago>()
                .HasOne(p => p.Dueno)
                .WithMany(d => d.Pagos)
                .HasForeignKey(p => p.DuenoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}