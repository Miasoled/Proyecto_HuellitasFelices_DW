using System.ComponentModel.DataAnnotations;

namespace HuellitasFelices.Models
{
    public class Mascota
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La especie es obligatoria")]
        [StringLength(50)]
        [Display(Name = "Especie")]
        public string Especie { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Raza")]
        public string? Raza { get; set; }

        [Range(0, 30)]
        [Display(Name = "Edad (años)")]
        public int Edad { get; set; }

        [Display(Name = "Peso (kg)")]
        [Range(0.1, 200)]
        public decimal Peso { get; set; }

        public bool Activo { get; set; } = true;

        [Display(Name = "Fecha de eliminación")]
        public DateTime? FechaEliminacion { get; set; }

        [Display(Name = "Fecha de registro")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [Display(Name = "Fecha de actualización")]
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

        // Llave foránea
        [Display(Name = "Dueño")]
        public int? DuenoId { get; set; }
        public Dueno? Dueno { get; set; }

        // Navegación
        public ICollection<Consulta> Consultas { get; set; } = new List<Consulta>();
    }
}