using HuellitasFelices.Areas.Identity.Pages.Account;
using HuellitasFelices.Data;
using HuellitasFelices.Models;
using HuellitasFelices.Models.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HuellitasFelices.Services;

public class AccountService : IAccountService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;

    public AccountService(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        AppDbContext context,
        IEmailService emailService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _emailService = emailService;
    }

    public async Task<PanelClienteDto?> ObtenerPanelClienteAsync(string email)
    {
        var dueno = _context.Duenos
            .FirstOrDefault(d => d.Email != null && d.Email.ToLower() == email.ToLower() && d.Activo);

        if (dueno == null) return null;

        var solicitudesAprobadas = _context.SolicitudesAdopcion
            .Include(s => s.AnimalAdopcion)
            .Where(s => s.Email != null && s.Email.ToLower() == email.ToLower() && s.Estado == "Aprobada" && s.Activo)
            .ToList();

        bool huboSincronizacion = false;
        foreach (var sol in solicitudesAprobadas)
        {
            if (sol.AnimalAdopcion != null)
            {
                string nombreBuscado = sol.AnimalAdopcion.Nombre.ToLower();
                var mascotaExiste = _context.Mascotas
                    .Any(m => m.Nombre.ToLower() == nombreBuscado && m.DuenoId == dueno.Id && m.Activo);

                if (!mascotaExiste)
                {
                    var nuevaMascota = new Mascota
                    {
                        Nombre = sol.AnimalAdopcion.Nombre,
                        Especie = sol.AnimalAdopcion.Especie,
                        Raza = sol.AnimalAdopcion.Raza ?? "Mestizo",
                        Sexo = "Macho",
                        FechaNacimiento = DateTime.UtcNow.AddYears(-sol.AnimalAdopcion.EdadAproximada),
                        Peso = 5.0m,
                        DuenoId = dueno.Id,
                        Activo = true,
                        FechaCreacion = DateTime.UtcNow,
                        FechaActualizacion = DateTime.UtcNow
                    };
                    _context.Mascotas.Add(nuevaMascota);
                    huboSincronizacion = true;
                }
            }
        }

        if (huboSincronizacion)
            await _context.SaveChangesAsync();

        var mascotas = _context.Mascotas
            .Where(m => m.DuenoId == dueno.Id && m.Activo)
            .ToList();

        var consultas = _context.Consultas
            .Include(c => c.Medicamentos).ThenInclude(cm => cm.Producto)
            .Include(c => c.Venta)
            .Where(c => mascotas.Select(m => m.Id).Contains(c.MascotaId) && c.Activo)
            .OrderByDescending(c => c.FechaConsulta)
            .Take(10)
            .ToList();

        var consultasPendientesPago = consultas
            .Where(c => c.Estado == "EnRevision" && c.Medicamentos.Any() && c.Venta == null)
            .ToList();

        var solicitudes = _context.SolicitudesAdopcion
            .Where(s => s.Email != null && s.Email.ToLower() == email.ToLower() && s.Activo)
            .OrderByDescending(s => s.FechaSolicitud)
            .Take(5)
            .ToList();

        var productosDestacados = _context.Productos
            .Include(p => p.Categoria)
            .Include(p => p.Inventarios)
            .Where(p => p.Activo && p.Inventarios.Any(i => i.StockActual > 0))
            .OrderByDescending(p => p.FechaCreacion)
            .Take(6)
            .ToList();

        return new PanelClienteDto
        {
            Dueno = dueno,
            Mascotas = mascotas,
            Consultas = consultas,
            ConsultasPendientesPago = consultasPendientesPago,
            Solicitudes = solicitudes,
            ProductosDestacados = productosDestacados
        };
    }

    public async Task<PanelDoctorDto?> ObtenerPanelDoctorAsync(string email)
    {
        var doctorUser = await _context.Empleados
            .FirstOrDefaultAsync(e => e.Email != null && e.Email.ToLower() == email.ToLower() && e.Activo);

        if (doctorUser == null) return null;

        var consultasPendientes = await _context.Consultas
            .Include(c => c.Mascota).ThenInclude(m => m!.Dueno)
            .Where(c => c.Activo && c.VeterinarioId == doctorUser.Id && c.Estado == "Pendiente")
            .OrderByDescending(c => c.FechaConsulta)
            .ToListAsync();

        var consultasEnRevision = await _context.Consultas
            .Include(c => c.Mascota).ThenInclude(m => m!.Dueno)
            .Where(c => c.Activo && c.VeterinarioId == doctorUser.Id && c.Estado == "EnRevision")
            .OrderByDescending(c => c.FechaConsulta)
            .ToListAsync();

        var consultasCompletadas = await _context.Consultas
            .Include(c => c.Mascota).ThenInclude(m => m!.Dueno)
            .Where(c => c.Activo && c.VeterinarioId == doctorUser.Id && c.Estado == "Completada")
            .OrderByDescending(c => c.FechaConsulta)
            .Take(10)
            .ToListAsync();

        return new PanelDoctorDto
        {
            Doctor = doctorUser,
            ConsultasPendientes = consultasPendientes,
            ConsultasEnRevision = consultasEnRevision,
            ConsultasCompletadas = consultasCompletadas
        };
    }

    public async Task<List<UsuarioConRolesDto>> ObtenerUsuariosAsync()
    {
        var usuarios = _userManager.Users.ToList();
        var resultado = new List<UsuarioConRolesDto>();

        foreach (var u in usuarios)
        {
            var roles = await _userManager.GetRolesAsync(u);
            resultado.Add(new UsuarioConRolesDto
            {
                UserId = u.Id,
                Email = u.Email ?? string.Empty,
                UserName = u.UserName,
                EmailConfirmed = u.EmailConfirmed,
                Roles = roles
            });
        }

        return resultado;
    }

    public async Task<(bool Exito, IEnumerable<string> Errores)> RegistrarUsuarioInternoAsync(RegisterInternoViewModel model)
    {
        if (model.Rol == "Doctor")
        {
            var totalDoctores = await _context.Empleados.CountAsync(e => e.Cargo == "Veterinario" && e.Activo);
            if (totalDoctores >= 3)
            {
                return (false, new[] { "Solo se permiten 3 doctores veterinarios en el sistema." });
            }
        }

        var user = new IdentityUser
        {
            UserName = model.Email,
            Email = model.Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, model.Rol);
            return (true, Enumerable.Empty<string>());
        }

        return (false, result.Errors.Select(e => e.Description));
    }

    public async Task<(bool Exito, IEnumerable<string> Errores)> CambiarPasswordAsync(IdentityUser user, string passwordActual, string nuevaPassword)
    {
        var result = await _userManager.ChangePasswordAsync(user, passwordActual, nuevaPassword);

        if (result.Succeeded)
        {
            try
            {
                await _emailService.EnviarCambioPasswordAsync(user.Email!, user.Email!);
            }
            catch { }
            return (true, Enumerable.Empty<string>());
        }

        return (false, result.Errors.Select(e => e.Description));
    }
}
