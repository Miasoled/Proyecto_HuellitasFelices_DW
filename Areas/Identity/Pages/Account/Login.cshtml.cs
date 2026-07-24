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

        public LoginModel(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
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

            var result = await _signInManager.PasswordSignInAsync(
                Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
                return LocalRedirect(returnUrl);

            if (result.RequiresTwoFactor)
            {
                var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
                if (user != null && await _userManager.IsInRoleAsync(user, "Cliente"))
                {
                    return RedirectToPage("LoginWith2fa", new
                    {
                        RememberMe = Input.RememberMe,
                        ReturnUrl = returnUrl
                    });
                }

                if (user != null)
                {
                    await _signInManager.SignInAsync(user, Input.RememberMe);
                    return LocalRedirect(returnUrl);
                }

                ModelState.AddModelError(string.Empty, "No se pudo completar la autenticación. Intente iniciar sesión nuevamente.");
                return Page();
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Su cuenta ha sido bloqueada temporalmente por demasiados intentos fallidos. Intente de nuevo en 15 minutos.");
                return Page();
            }

            if (result.IsNotAllowed)
            {
                ModelState.AddModelError(string.Empty, "Debes confirmar tu correo electrónico antes de iniciar sesión. Revisa tu bandeja de entrada.");
                return Page();
            }

            ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
            return Page();
        }
    }
}
