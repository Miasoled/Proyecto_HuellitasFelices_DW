using Microsoft.EntityFrameworkCore;
using HuellitasFelices.Data;
using HuellitasFelices.Models;
using HuellitasFelices.ViewModels;
using HuellitasFelices.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace HuellitasFelices.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;
        private readonly IEmailSender _emailSender;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
            _emailSender = emailSender;
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

            if (result.IsNotAllowed)
            {
                ModelState.AddModelError(string.Empty, "Debes confirmar tu correo electrónico antes de iniciar sesión. Revisa tu bandeja de entrada.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
            return View(model);
        }

        // ── LOGIN EXTERNO (Google) ───────────────────────────────────────────

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            returnUrl ??= "/";

            if (remoteError != null)
            {
                TempData["Error"] = $"Error del proveedor externo: {remoteError}";
                return RedirectToAction(nameof(Login));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                TempData["Error"] = "No se pudo obtener la información de inicio de sesión externo.";
                return RedirectToAction(nameof(Login));
            }

            // 1) ¿Ya existe un login externo vinculado a un usuario? -> iniciar sesión directo
            var result = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (result.Succeeded)
                return LocalRedirect(returnUrl);

            // 2) No existe el login externo todavía -> buscar/crear el usuario por email
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var nombre = info.Principal.FindFirstValue(ClaimTypes.Name) ?? email ?? "Usuario Google";

            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Tu cuenta de Google no tiene un correo asociado.";
                return RedirectToAction(nameof(Login));
            }

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                // Usuario nuevo: se registra automáticamente como Cliente
                user = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    TempData["Error"] = string.Join(" ", createResult.Errors.Select(e => e.Description));
                    return RedirectToAction(nameof(Login));
                }

                await _userManager.AddToRoleAsync(user, "Cliente");

                var duenoExistente = _context.Duenos.FirstOrDefault(d => d.Email == email);
                if (duenoExistente == null)
                {
                    _context.Duenos.Add(new Dueno
                    {
                        Nombre = nombre,
                        Email = email,
                        Telefono = string.Empty,
                        Activo = true,
                        FechaCreacion = DateTime.UtcNow,
                        FechaActualizacion = DateTime.UtcNow
                    });
                    await _context.SaveChangesAsync();
                }
            }

            // Vincular el login de Google a este usuario para la próxima vez
            var addLoginResult = await _userManager.AddLoginAsync(user, info);
            if (!addLoginResult.Succeeded)
            {
                TempData["Error"] = "No se pudo vincular la cuenta de Google.";
                return RedirectToAction(nameof(Login));
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            return LocalRedirect(returnUrl);
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

                // Generar token de confirmación de email
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                // Generar enlace de confirmación
                var confirmationLink = Url.Action("ConfirmEmail", "Account", 
                    new { userId = user.Id, token = token }, Request.Scheme);

                // Enviar el correo de confirmación utilizando plantilla centralizada
                await _emailSender.SendEmailAsync(
                    model.Email,
                    "Confirma tu correo electrónico - Huellitas Felices",
                    EmailTemplates.WelcomeTemplate(model.Email, confirmationLink!));

                return RedirectToAction("RegisterConfirmation", new { email = model.Email });
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        // ── CONFIRMACIÓN DE REGISTRO / EMAIL ──────────────────────────────────

        [HttpGet]
        [AllowAnonymous]
        public IActionResult RegisterConfirmation(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Email = email;
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                ViewBag.Succeeded = false;
                ViewBag.Message = "El enlace de confirmación es inválido.";
                return View();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.Succeeded = false;
                ViewBag.Message = "El usuario no existe.";
                return View();
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                ViewBag.Succeeded = true;
                return View();
            }

            ViewBag.Succeeded = false;
            ViewBag.Message = "El enlace ha expirado o el token de confirmación es inválido.";
            return View();
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
            if (user == null)
                return RedirectToAction("Index", "Home");

            // Búsqueda insensible a mayúsculas para el dueño
            var dueno = _context.Duenos
                .FirstOrDefault(d => d.Email != null && user.Email != null && d.Email.ToLower() == user.Email.ToLower() && d.Activo);

            if (dueno == null)
                return RedirectToAction("Index", "Home");

            // Sincronizar de forma proactiva cualquier adopción aprobada previamente
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
                            Edad = sol.AnimalAdopcion.EdadAproximada,
                            Peso = 5.0m, // Peso base
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

        // ── RECUPERACIÓN DE CONTRASEÑA ────────────────────────────────────────

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // Por seguridad, no revelamos si el usuario existe o no
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action("ResetPassword", "Account", new { token, email = model.Email }, Request.Scheme);

            await _emailSender.SendEmailAsync(
                model.Email,
                "Restablecer contraseña - Huellitas Felices",
                EmailTemplates.ForgotPasswordTemplate(callbackUrl!));

            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string? token = null, string? email = null)
        {
            if (token == null || email == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new ResetPasswordViewModel { Token = token, Email = email };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Por seguridad, redirigir sin dar pistas
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
    }
}