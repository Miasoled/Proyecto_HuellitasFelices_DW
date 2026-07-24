using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HuellitasFelices.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginWith2faModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;

        public LoginWith2faModel(SignInManager<IdentityUser> signInManager)
        {
            _signInManager = signInManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; } = string.Empty;

        public class InputModel
        {
            [Required(ErrorMessage = "El código es obligatorio")]
            [StringLength(7, ErrorMessage = "El código debe tener entre 6 y 7 caracteres", MinimumLength = 6)]
            [Display(Name = "Código de autenticación")]
            public string AuthenticatorCode { get; set; } = string.Empty;

            [Display(Name = "Recordar este dispositivo")]
            public bool RememberMachine { get; set; }

            public bool RememberMe { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(bool rememberMe, string? returnUrl = null)
        {
            RememberMe = rememberMe;
            ReturnUrl = returnUrl ?? Url.Content("~/");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(bool rememberMe, string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
                return Page();

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync()
                ?? throw new InvalidOperationException("No se pudo cargar el usuario de autenticación de dos factores.");

            var authenticatorCode = Input.AuthenticatorCode
                .Replace(" ", string.Empty)
                .Replace("-", string.Empty);

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(
                authenticatorCode, Input.RememberMachine, rememberMe);

            if (result.Succeeded)
                return LocalRedirect(returnUrl);

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Su cuenta ha sido bloqueada temporalmente por demasiados intentos fallidos.");
                return Page();
            }

            ModelState.AddModelError(string.Empty, "Código de autenticación inválido. Intente de nuevo.");
            return Page();
        }
    }
}
