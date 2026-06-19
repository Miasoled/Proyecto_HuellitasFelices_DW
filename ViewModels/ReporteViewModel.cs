namespace HuellitasFelices.ViewModels
{
    public class ReporteViewModel
    {
        // Totales generales
        public decimal TotalIngresosPagos { get; set; }
        public decimal TotalIngresosConsultas { get; set; }
        public decimal PromedioMontoPago { get; set; }
        public decimal PromedioCostoConsulta { get; set; }

        // Conteos
        public int TotalMascotas { get; set; }
        public int TotalDuenos { get; set; }
        public int TotalConsultas { get; set; }
        public int TotalPagos { get; set; }
        public int PagosActivos { get; set; }
        public int PagosInactivos { get; set; }
        public int MascotasActivas { get; set; }
        public int MascotasInactivas { get; set; }

        // Agrupaciones
        public List<ResumenMensual> ConsultasPorMes { get; set; } = new();
        public List<ResumenMotivo> Top10Motivos { get; set; } = new();
        public List<ResumenDueno> Top10DuenosConMascotas { get; set; } = new();
        public List<ResumenCargo> EmpleadosPorCargo { get; set; } = new();
        public List<ResumenMetodoPago> PagosPorMetodo { get; set; } = new();
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

    public class ResumenMetodoPago
    {
        public string MetodoPago { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal Total { get; set; }
    }
}
