using HuellitasFelices.Areas.Identity.Pages.Account;
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

        public AccountController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IAccountService accountService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _accountService = accountService;
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
