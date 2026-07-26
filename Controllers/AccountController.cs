using HuellitasFelices.Areas.Identity.Pages.Account;
using HuellitasFelices.Models.Dtos;
using HuellitasFelices.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HuellitasFelices.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAccountService _accountService;
        private readonly IAuditService _auditService;

        public AccountController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IAccountService accountService,
            IAuditService auditService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _accountService = accountService;
            _auditService = auditService;
        }

        // ── PANEL DEL CLIENTE ─────────────────────────────────────────────────

        [HttpGet]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> MiPanel()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.Email == null)
                return RedirectToAction("Index", "Home");

            var panel = await _accountService.ObtenerPanelClienteAsync(user.Email);
            if (panel == null)
                return RedirectToAction("Index", "Home");

            ViewBag.Dueno = panel.Dueno;
            ViewBag.Mascotas = panel.Mascotas;
            ViewBag.Consultas = panel.Consultas;
            ViewBag.ConsultasPendientesPago = panel.ConsultasPendientesPago;
            ViewBag.Solicitudes = panel.Solicitudes;
            ViewBag.ProductosDestacados = panel.ProductosDestacados;

            return View();
        }

        // ── PANEL DEL DOCTOR ────────────────────────────────────────────────

        [HttpGet]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> PanelDoctor()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.Email == null)
                return RedirectToAction("Index", "Home");

            var panel = await _accountService.ObtenerPanelDoctorAsync(user.Email);
            if (panel == null)
                return RedirectToAction("Index", "Home");

            ViewBag.Doctor = panel.Doctor;
            ViewBag.Pendientes = panel.ConsultasPendientes;
            ViewBag.EnRevision = panel.ConsultasEnRevision;
            ViewBag.Completadas = panel.ConsultasCompletadas;

            return View();
        }

        // ── GESTIÓN DE USUARIOS (solo Administrador) ──────────────────────────

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Usuarios()
        {
            var usuarios = await _accountService.ObtenerUsuariosAsync();
            return View(usuarios);
        }

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EditarRoles(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null) return NotFound();

            return View(new EditarRolesUsuarioViewModel
            {
                UserId = usuario.Id,
                Email = usuario.Email ?? usuario.UserName ?? "Usuario sin correo",
                RolesSeleccionados = (await _userManager.GetRolesAsync(usuario)).ToList(),
                RolesDisponibles = _roleManager.Roles
                    .Where(r => r.Name != null)
                    .OrderBy(r => r.Name)
                    .Select(r => r.Name!)
                    .ToList()
            });
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarRoles(EditarRolesUsuarioViewModel model)
        {
            var usuario = await _userManager.FindByIdAsync(model.UserId);
            if (usuario == null) return NotFound();

            model.RolesSeleccionados ??= new List<string>();
            model.RolesDisponibles = _roleManager.Roles
                .Where(r => r.Name != null)
                .OrderBy(r => r.Name)
                .Select(r => r.Name!)
                .ToList();
            model.Email = usuario.Email ?? usuario.UserName ?? "Usuario sin correo";

            if (model.RolesSeleccionados.Count == 0)
                ModelState.AddModelError(nameof(model.RolesSeleccionados), "Selecciona al menos un rol.");

            if (model.RolesSeleccionados.Any(rol => !model.RolesDisponibles.Contains(rol)))
                ModelState.AddModelError(nameof(model.RolesSeleccionados), "Se seleccionó un rol no válido.");

            var administradorActual = await _userManager.GetUserAsync(User);
            var rolesActuales = await _userManager.GetRolesAsync(usuario);
            var quitandoAdministrador = rolesActuales.Contains("Administrador") &&
                !model.RolesSeleccionados.Contains("Administrador");

            if (administradorActual?.Id == usuario.Id && quitandoAdministrador)
                ModelState.AddModelError(string.Empty, "No puedes quitarte tu propio rol de Administrador.");

            if (quitandoAdministrador)
            {
                var administradores = await _userManager.GetUsersInRoleAsync("Administrador");
                if (administradores.Count <= 1)
                    ModelState.AddModelError(string.Empty, "Debe existir al menos un usuario con rol Administrador.");
            }

            if (!ModelState.IsValid) return View(model);

            var rolesAEliminar = rolesActuales.Except(model.RolesSeleccionados).ToList();
            var rolesAAgregar = model.RolesSeleccionados.Except(rolesActuales).ToList();

            var eliminar = await _userManager.RemoveFromRolesAsync(usuario, rolesAEliminar);
            var agregar = eliminar.Succeeded
                ? await _userManager.AddToRolesAsync(usuario, rolesAAgregar)
                : eliminar;

            if (!agregar.Succeeded)
            {
                foreach (var error in agregar.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            await _auditService.LogAsync(
                accion: "RolesUsuarioActualizados",
                entidad: "IdentityUser",
                usuarioId: administradorActual?.Id,
                usuarioEmail: administradorActual?.Email,
                direccionIP: HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                valorAnterior: string.Join(", ", rolesActuales),
                valorNuevo: $"Usuario: {usuario.Email}; Roles: {string.Join(", ", model.RolesSeleccionados)}");

            TempData["Mensaje"] = $"Roles actualizados para {usuario.Email}.";
            return RedirectToAction(nameof(Usuarios));
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

            var (exito, errores) = await _accountService.RegistrarUsuarioInternoAsync(model);

            if (exito)
            {
                var administrador = await _userManager.GetUserAsync(User);
                await _auditService.LogAsync(
                    accion: "UsuarioInternoCreado",
                    entidad: "IdentityUser",
                    usuarioId: administrador?.Id,
                    usuarioEmail: administrador?.Email,
                    direccionIP: HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    valorNuevo: $"Usuario: {model.Email}; Rol: {model.Rol}",
                    descripcion: $"El administrador creó una cuenta interna y asignó el rol {model.Rol}.");
                TempData["Mensaje"] = $"Usuario {model.Email} creado con rol {model.Rol}.";
                return RedirectToAction(nameof(Usuarios));
            }

            foreach (var error in errores)
                ModelState.AddModelError(string.Empty, error);

            var rolesList = _roleManager.Roles.Where(r => r.Name != "Cliente").ToList();
            ViewBag.Roles = new SelectList(rolesList, "Name", "Name");
            return View(model);
        }

        // ── CAMBIAR CONTRASEÑA ────────────────────────────────────────────

        [HttpGet]
        [Authorize]
        public IActionResult CambiarPassword()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarPassword(CambiarPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Index", "Home");

            var (exito, errores) = await _accountService.CambiarPasswordAsync(user, model.PasswordActual, model.NuevaPassword);

            if (exito)
            {
                TempData["Mensaje"] = "Contraseña cambiada correctamente. Se ha enviado una notificación a tu correo.";
                return RedirectToAction("MiPanel");
            }

            foreach (var error in errores)
                ModelState.AddModelError(string.Empty, error);

            return View(model);
        }
    }
}
