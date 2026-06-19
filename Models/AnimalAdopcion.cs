using System.ComponentModel.DataAnnotations;

namespace HuellitasFelices.Models
{
    public class AnimalAdopcion
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Especie")]
        public string Especie { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Raza")]
        public string? Raza { get; set; }

        [Range(0, 30)]
        [Display(Name = "Edad aproximada (años)")]
        public int EdadAproximada { get; set; }

        [StringLength(500)]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Display(Name = "Disponible para adopción")]
        public bool Disponible { get; set; } = true;

        [StringLength(300)]
        [Display(Name = "Foto")]
        public string? FotoUrl { get; set; }

        public bool Activo { get; set; } = true;

        [Display(Name = "Fecha de eliminación")]
        public DateTime? FechaEliminacion { get; set; }

        [Display(Name = "Fecha de ingreso")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [Display(Name = "Fecha de actualización")]
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

        // Navegación
        public ICollection<SolicitudAdopcion> Solicitudes { get; set; } = new List<SolicitudAdopcion>();
    }
}