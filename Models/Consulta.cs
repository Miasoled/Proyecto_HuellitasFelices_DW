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

        [StringLength(500, ErrorMessage = "Los síntomas no pueden superar los 500 caracteres")]
        [Display(Name = "Síntomas")]
        public string? Sintomas { get; set; }

        [StringLength(500)]
        [Display(Name = "Diagnóstico")]
        public string? Diagnostico { get; set; }

        [Required]
        [StringLength(30)]
        [Display(Name = "Estado")]
        public string Estado { get; set; } = "Pendiente";  // Pendiente, EnRevision, Completada

        [Range(0, 99999)]
        [Display(Name = "Costo")]
        public decimal Costo { get; set; }

        public bool Activo { get; set; } = true;

        [Display(Name = "Fecha de eliminación")]
        public DateTime? FechaEliminacion { get; set; }

        [StringLength(100)]
        [Display(Name = "Eliminado por")]
        public string? EliminadoPor { get; set; }

        [Display(Name = "Fecha de consulta")]
        public DateTime FechaConsulta { get; set; } = DateTime.UtcNow;

        [Display(Name = "Fecha de registro")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [Display(Name = "Fecha de actualización")]
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

        // Veterinario asignado
        [Display(Name = "Veterinario")]
        public int? VeterinarioId { get; set; }
        public Empleado? Veterinario { get; set; }

        // Llave foránea
        [Required]
        [Display(Name = "Mascota")]
        public int MascotaId { get; set; }
        public Mascota? Mascota { get; set; }

        [Display(Name = "Sucursal")]
        public int? SucursalId { get; set; }
        public Sucursal? Sucursal { get; set; }

        // Navegación
        public ICollection<Tratamiento> Tratamientos { get; set; } = new List<Tratamiento>();
        public ICollection<ConsultaMedicamento> Medicamentos { get; set; } = new List<ConsultaMedicamento>();
        public Venta? Venta { get; set; }
    }
}