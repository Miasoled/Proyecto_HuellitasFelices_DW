using System.ComponentModel.DataAnnotations;

namespace HuellitasFelices.Models
{
    public class Consulta
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El motivo es obligatorio")]
        [StringLength(200)]
        [Display(Name = "Motivo")]
        public string Motivo { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Diagnóstico")]
        public string? Diagnostico { get; set; }

        [Range(0, 99999)]
        [Display(Name = "Costo")]
        public decimal Costo { get; set; }

        public bool Activo { get; set; } = true;

        [Display(Name = "Fecha de eliminación")]
        public DateTime? FechaEliminacion { get; set; }

        [Display(Name = "Fecha de consulta")]
        public DateTime FechaConsulta { get; set; } = DateTime.UtcNow;

        [Display(Name = "Fecha de registro")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [Display(Name = "Fecha de actualización")]
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

        // Llave foránea
        [Required]
        [Display(Name = "Mascota")]
        public int MascotaId { get; set; }
        public Mascota? Mascota { get; set; }

        // Navegación
        public ICollection<Tratamiento> Tratamientos { get; set; } = new List<Tratamiento>();
    }
}