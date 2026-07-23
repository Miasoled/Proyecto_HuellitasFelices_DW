namespace HuellitasFelices.Models
{
    public class ReporteViewModel
    {
        // Totales generales
        public decimal TotalIngresosConsultas { get; set; }
        public decimal PromedioCostoConsulta { get; set; }

        // Conteos
        public int TotalMascotas { get; set; }
        public int TotalDuenos { get; set; }
        public int TotalConsultas { get; set; }
        public int MascotasActivas { get; set; }
        public int MascotasInactivas { get; set; }

        // Agrupaciones
        public List<ResumenMensual> ConsultasPorMes { get; set; } = new();
        public List<ResumenMotivo> Top10Motivos { get; set; } = new();
        public List<ResumenDueno> Top10DuenosConMascotas { get; set; } = new();
        public List<ResumenCargo> EmpleadosPorCargo { get; set; } = new();
    }

    public class ResumenMensual
    {
        public int Anio { get; set; }
        public int Mes { get; set; }
        public string NombreMes => new DateTime(Anio, Mes, 1).ToString("MMMM yyyy");
        public int TotalConsultas { get; set; }
        public decimal TotalIngresos { get; set; }
    }

    public class ResumenMotivo
    {
        public string Motivo { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal TotalIngresos { get; set; }
    }

    public class ResumenDueno
    {
        public string Nombre { get; set; } = string.Empty;
        public int TotalMascotas { get; set; }
        public string? Email { get; set; }
    }

    public class ResumenCargo
    {
        public string Cargo { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal SalarioPromedio { get; set; }
    }
}
