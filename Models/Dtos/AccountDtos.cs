namespace HuellitasFelices.Models.Dtos;

public class PanelClienteDto
{
    public Dueno Dueno { get; set; } = null!;
    public List<Mascota> Mascotas { get; set; } = new();
    public List<Consulta> Consultas { get; set; } = new();
    public List<Consulta> ConsultasPendientesPago { get; set; } = new();
    public List<SolicitudAdopcion> Solicitudes { get; set; } = new();
    public List<Producto> ProductosDestacados { get; set; } = new();
}

public class PanelDoctorDto
{
    public Empleado Doctor { get; set; } = null!;
    public List<Consulta> ConsultasPendientes { get; set; } = new();
    public List<Consulta> ConsultasEnRevision { get; set; } = new();
    public List<Consulta> ConsultasCompletadas { get; set; } = new();
}

public class UsuarioConRolesDto
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public bool EmailConfirmed { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
}

public class EditarRolesUsuarioViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> RolesSeleccionados { get; set; } = new();
    public List<string> RolesDisponibles { get; set; } = new();
}
