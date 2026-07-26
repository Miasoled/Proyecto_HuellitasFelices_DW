using HuellitasFelices.Data;
using HuellitasFelices.Models;
using HuellitasFelices.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HuellitasFelices.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IAuditService _auditService;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            AppDbContext context,
            IEmailService emailService,
            IAuditService auditService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _emailService = emailService;
            _auditService = auditService;
        }

        [BindProperty]
        public RegisterViewModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");

            if (!ModelState.IsValid)
                return Page();

            var user = new IdentityUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                PhoneNumber = Input.Telefono
            };

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Cliente");
                await _auditService.LogAsync("UsuarioRegistrado", "IdentityUser", usuarioId: user.Id,
                    usuarioEmail: user.Email,
                    direccionIP: HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    valorNuevo: "Rol: Cliente", descripcion: "Nueva cuenta registrada, pendiente de confirmación de correo.");

                var dueno = new Dueno
                {
                    Nombre = Input.NombreCompleto,
                    Telefono = Input.Telefono,
                    Email = Input.Email,
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow,
                    FechaActualizacion = DateTime.UtcNow
                };
                _context.Duenos.Add(dueno);
                await _context.SaveChangesAsync();

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                var confirmationLink = Url.Page(
                    "./ConfirmEmail",
                    pageHandler: null,
                    values: new { userId = user.Id, token = token },
                    protocol: Request.Scheme);

                await _emailService.EnviarConfirmacionCorreoAsync(
                    Input.Email,
                    Input.NombreCompleto,
                    confirmationLink!);

                return RedirectToPage("./RegisterConfirmation", new { email = Input.Email });
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return Page();
        }
    }
}
