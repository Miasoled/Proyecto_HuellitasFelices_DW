using HuellitasFelices.Data;
using HuellitasFelices.Models;
using HuellitasFelices.Areas.Identity.Pages.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HuellitasFelices.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;

        public AccountController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // ── PANEL DEL CLIENTE ─────────────────────────────────────────────────

        [HttpGet]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> MiPanel()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Index", "Home");

            var dueno = _context.Duenos
                .FirstOrDefault(d => d.Email != null && user.Email != null && d.Email.ToLower() == user.Email.ToLower() && d.Activo);

            if (dueno == null)
                return RedirectToAction("Index", "Home");

            var solicitudesAprobadas = _context.SolicitudesAdopcion
                .Include(s => s.AnimalAdopcion)
                .Where(s => s.Email != null && user.Email != null && s.Email.ToLower() == user.Email.ToLower() && s.Estado == "Aprobada" && s.Activo)
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
            {
                await _context.SaveChangesAsync();
            }

            var mascotas = _context.Mascotas
                .Where(m => m.DuenoId == dueno.Id && m.Activo)
                .ToList();

            var consultas = _context.Consultas
                .Where(c => mascotas.Select(m => m.Id).Contains(c.MascotaId) && c.Activo)
                .OrderByDescending(c => c.FechaConsulta)
                .Take(5)
                .ToList();

            var solicitudes = _context.SolicitudesAdopcion
                .Where(s => s.Email != null && user.Email != null && s.Email.ToLower() == user.Email.ToLower() && s.Activo)
                .OrderByDescending(s => s.FechaSolicitud)
                .Take(5)
                .ToList();

            ViewBag.Dueno = dueno;
            ViewBag.Mascotas = mascotas;
            ViewBag.Consultas = consultas;
            ViewBag.Solicitudes = solicitudes;

            return View();
        }

        // ── GESTIÓN DE USUARIOS (solo Administrador) ──────────────────────────

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Usuarios()
        {
            var usuarios = _userManager.Users.ToList();
            var modelo = new List<(IdentityUser usuario, IList<string> roles)>();

            foreach (var u in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(u);
                modelo.Add((u, roles));
            }

            return View(modelo);
        }

        // ── REGISTRO INTERNO (solo Administrador) ─────────────────────────────

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public IActionResult RegisterInterno()
        {
            var roles = _roleManager.Roles
                .Where(r => r.Name != "Cliente")
                .ToList();
            ViewBag.Roles = new SelectList(roles, "Name", "Name");
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterInterno(RegisterInternoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var roles = _roleManager.Roles.Where(r => r.Name != "Cliente").ToList();
                ViewBag.Roles = new SelectList(roles, "Name", "Name");
                return View(model);
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
                TempData["Mensaje"] = $"Usuario {model.Email} creado con rol {model.Rol}.";
                return RedirectToAction(nameof(Usuarios));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            var rolesList = _roleManager.Roles.Where(r => r.Name != "Cliente").ToList();
            ViewBag.Roles = new SelectList(rolesList, "Name", "Name");
            return View(model);
        }
    }
}
