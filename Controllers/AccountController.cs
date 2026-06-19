using HuellitasFelices.Data;
using HuellitasFelices.Models;
using HuellitasFelices.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HuellitasFelices.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
        }

        // ── LOGIN ─────────────────────────────────────────────────────────────

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid) return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
                return LocalRedirect(returnUrl ?? "/");

            ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
            return View(model);
        }

        // ── REGISTRO PÚBLICO (Clientes) ───────────────────────────────────────

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true,
                PhoneNumber = model.Telefono
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Asignar rol Cliente automáticamente
                await _userManager.AddToRoleAsync(user, "Cliente");

                // Crear el dueño vinculado al usuario
                var dueno = new Dueno
                {
                    Nombre = model.NombreCompleto,
                    Telefono = model.Telefono,
                    Email = model.Email,
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow,
                    FechaActualizacion = DateTime.UtcNow
                };
                _context.Duenos.Add(dueno);
                await _context.SaveChangesAsync();

                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("MiPanel", "Account");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
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

        // ── PANEL DEL CLIENTE ─────────────────────────────────────────────────

        [HttpGet]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> MiPanel()
        {
            var user = await _userManager.GetUserAsync(User);
            var dueno = _context.Duenos
                .FirstOrDefault(d => d.Email == user!.Email && d.Activo);

            if (dueno == null)
                return RedirectToAction("Index", "Home");

            var mascotas = _context.Mascotas
                .Where(m => m.DuenoId == dueno.Id && m.Activo)
                .ToList();

            var consultas = _context.Consultas
                .Where(c => mascotas.Select(m => m.Id).Contains(c.MascotaId) && c.Activo)
                .OrderByDescending(c => c.FechaConsulta)
                .Take(5)
                .ToList();

            var solicitudes = _context.SolicitudesAdopcion
                .Where(s => s.Email == user!.Email && s.Activo)
                .OrderByDescending(s => s.FechaSolicitud)
                .Take(5)
                .ToList();

            ViewBag.Dueno = dueno;
            ViewBag.Mascotas = mascotas;
            ViewBag.Consultas = consultas;
            ViewBag.Solicitudes = solicitudes;

            return View();
        }

        // ── LOGOUT ────────────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // ── ACCESS DENIED ─────────────────────────────────────────────────────

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
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
    }
}
