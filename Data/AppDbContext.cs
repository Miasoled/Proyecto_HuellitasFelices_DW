using HuellitasFelices.Models;
using Microsoft.EntityFrameworkCore;

namespace HuellitasFelices.Data
{
    public class AppDbContext : DbContext
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Mascota -> Dueno (un dueño puede tener muchas mascotas)
            modelBuilder.Entity<Mascota>()
                .HasOne(m => m.Dueno)
                .WithMany(d => d.Mascotas)
                .HasForeignKey(m => m.DuenoId)
                .OnDelete(DeleteBehavior.SetNull);

            // Consulta -> Mascota (una mascota puede tener muchas consultas)
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
        }
    }
}