using HuellitasFelices.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HuellitasFelices.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAuditService _auditService;
        private readonly IEmailService _emailService;

        public LoginModel(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            IAuditService auditService,
            IEmailService emailService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _auditService = auditService;
            _emailService = emailService;
        }

        [BindProperty]
        public LoginViewModel Input { get; set; } = new();

        public string ReturnUrl { get; set; } = string.Empty;

        public async Task OnGetAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
                return Page();

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            var result = await _signInManager.PasswordSignInAsync(
                Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                await _auditService.LogAsync(
                    accion: "LoginExitoso",
                    entidad: "IdentityUser",
                    usuarioEmail: Input.Email,
                    direccionIP: ip,
                    descripcion: $"Inicio de sesión exitoso para {Input.Email}");
                return LocalRedirect(returnUrl);
            }

            if (result.RequiresTwoFactor)
            {
                return RedirectToPage("LoginWith2fa", new { RememberMe = Input.RememberMe, ReturnUrl = returnUrl });
            }

            if (result.IsLockedOut)
            {
                await _auditService.LogAsync(
                    accion: "CuentaBloqueada",
                    entidad: "IdentityUser",
                    usuarioEmail: Input.Email,
                    direccionIP: ip,
                    descripcion: $"Cuenta bloqueada por intentos fallidos: {Input.Email}",
                    nivel: "Warning");

                try
                {
                    await _emailService.EnviarCuentaBloqueadaAsync(Input.Email, Input.Email);
                }
                catch { }

                ModelState.AddModelError(string.Empty, "Su cuenta ha sido bloqueada temporalmente por demasiados intentos fallidos. Intente de nuevo en 15 minutos.");
                return Page();
            }

            if (result.IsNotAllowed)
            {
                await _auditService.LogAsync(
                    accion: "LoginNoPermitido",
                    entidad: "IdentityUser",
                    usuarioEmail: Input.Email,
                    direccionIP: ip,
                    descripcion: $"Login no permitido (correo no confirmado): {Input.Email}",
                    nivel: "Warning");

                ModelState.AddModelError(string.Empty, "Debes confirmar tu correo electrónico antes de iniciar sesión. Revisa tu bandeja de entrada.");
                return Page();
            }

            await _auditService.LogAsync(
                accion: "LoginFallido",
                entidad: "IdentityUser",
                usuarioEmail: Input.Email,
                direccionIP: ip,
                descripcion: $"Intento de login fallido para {Input.Email}",
                nivel: "Warning");

            ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
            return Page();
        }
    }
}
