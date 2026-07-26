using HuellitasFelices.Data;
using HuellitasFelices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace HuellitasFelices.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginCallbackModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext _context;

        public ExternalLoginCallbackModel(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            AppDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(string? returnUrl = null, string? remoteError = null)
        {
            returnUrl ??= Url.Content("~/");

            if (remoteError != null)
            {
                TempData["Error"] = $"Error del proveedor externo: {remoteError}";
                return RedirectToPage("./Login");
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                TempData["Error"] = "No se pudo obtener la información de inicio de sesión externo.";
                return RedirectToPage("./Login");
            }

            var result = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: false);

            if (result.Succeeded)
                return LocalRedirect(returnUrl);

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var nombre = info.Principal.FindFirstValue(ClaimTypes.Name) ?? email ?? "Usuario Google";

            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Tu cuenta de Google no tiene un correo asociado.";
                return RedirectToPage("./Login");
            }

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
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
                    return RedirectToPage("./Login");
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

            var addLoginResult = await _userManager.AddLoginAsync(user, info);
            if (!addLoginResult.Succeeded)
            {
                TempData["Error"] = "No se pudo vincular la cuenta de Google.";
                return RedirectToPage("./Login");
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            return LocalRedirect(returnUrl);
        }
    }
}
