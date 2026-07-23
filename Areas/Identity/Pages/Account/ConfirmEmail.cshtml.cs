using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HuellitasFelices.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;

        public ConfirmEmailModel(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public bool Succeeded { get; set; }
        public string Message { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(string? userId, string? token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                Succeeded = false;
                Message = "El enlace de confirmación es inválido.";
                return Page();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                Succeeded = false;
                Message = "El usuario no existe.";
                return Page();
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                Succeeded = true;
                return Page();
            }

            Succeeded = false;
            Message = "El enlace ha expirado o el token de confirmación es inválido.";
            return Page();
        }
    }
}
