using System.ComponentModel.DataAnnotations;

namespace HuellitasFelices.Models
{
    public class Empleado
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        [Display(Name = "Nombre completo")]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Cargo")]
        public string Cargo { get; set; } = string.Empty; // Veterinario, Asistente, Recepcionista

        [StringLength(20)]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [Range(500, 99999)]
        [Display(Name = "Salario")]
        public decimal Salario { get; set; }

        public bool Activo { get; set; } = true;

        [Display(Name = "Fecha de ingreso")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}