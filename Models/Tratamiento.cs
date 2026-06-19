using System.ComponentModel.DataAnnotations;

namespace HuellitasFelices.Models
{
    public class Tratamiento
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(150)]
        [Display(Name = "Nombre del tratamiento")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(300)]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Required]
        [Range(0.01, 99999)]
        [Display(Name = "Costo")]
        public decimal Costo { get; set; }

        [StringLength(100)]
        [Display(Name = "Medicamento utilizado")]
        public string? Medicamento { get; set; }

        // Auditoría y eliminación lógica
        public bool Activo { get; set; } = true;

        [Display(Name = "Fecha de eliminación")]
        public DateTime? FechaEliminacion { get; set; }

        [Display(Name = "Fecha de registro")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [Display(Name = "Fecha de actualización")]
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

        // Llave foránea
        [Required]
        [Display(Name = "Consulta")]
        public int ConsultaId { get; set; }
        public Consulta? Consulta { get; set; }
    }
}