using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HuellitasFelices.Areas.Identity.Pages.Account
{
    [Authorize]
    public class TwoFactorAuthenticationModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        
        public bool IsEnabled { get; set; }
        public string[]? RecoveryCodes { get; set; }
        
        public TwoFactorAuthenticationModel(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }
        
        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
            
            IsEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            
            if (TempData["RecoveryCodes"] is string[] codes)
                RecoveryCodes = codes;
            
            return Page();
        }
        
        public async Task<IActionResult> OnPostResetAuthenticatorAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
            
            await _userManager.SetTwoFactorEnabledAsync(user, false);
            await _userManager.ResetAuthenticatorKeyAsync(user);
            
            return RedirectToPage("EnableAuthenticator");
        }
        
        public async Task<IActionResult> OnPostDisable2faAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
            
            var result = await _userManager.SetTwoFactorEnabledAsync(user, false);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = "Error al desactivar MFA.";
                return RedirectToPage();
            }
            
            TempData["SuccessMessage"] = "MFA desactivado correctamente.";
            return RedirectToPage();
        }
    }
}
