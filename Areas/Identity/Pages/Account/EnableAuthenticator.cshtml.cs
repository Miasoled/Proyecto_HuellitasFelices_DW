using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace HuellitasFelices.Areas.Identity.Pages.Account
{
    [Authorize]
    public class EnableAuthenticatorModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        
        public string SharedKey { get; set; } = string.Empty;
        public string AuthenticatorUri { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        
        public EnableAuthenticatorModel(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }
        
        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
            
            await LoadSharedKeyAndQrCodeUriAsync(user);
            return Page();
        }
        
        public async Task<IActionResult> OnPostAsync(string sharedKey, string authenticatorUri, string code)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
            
            if (string.IsNullOrWhiteSpace(code))
            {
                ErrorMessage = "Ingrese el código de verificación.";
                await LoadSharedKeyAndQrCodeUriAsync(user);
                return Page();
            }
            
            code = code.Replace(" ", "").Replace("-", "");
            
            var isCodeValid = await _userManager.VerifyTwoFactorTokenAsync(
                user, _userManager.Options.Tokens.AuthenticatorTokenProvider, code);
            
            if (!isCodeValid)
            {
                ErrorMessage = "Código inválido. Verifique e intente de nuevo.";
                await LoadSharedKeyAndQrCodeUriAsync(user);
                return Page();
            }
            
            await _userManager.SetTwoFactorEnabledAsync(user, true);
            
            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            
            TempData["RecoveryCodes"] = recoveryCodes?.ToArray();
            TempData["SuccessMessage"] = "Autenticación multifactor activada correctamente.";
            
            return RedirectToPage("TwoFactorAuthentication");
        }
        
        private async Task LoadSharedKeyAndQrCodeUriAsync(IdentityUser user)
        {
            var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(unformattedKey))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            }
            
            SharedKey = FormatKey(unformattedKey!);
            
            var email = await _userManager.GetEmailAsync(user);
            AuthenticatorUri = GenerateQrCodeUri(email!, unformattedKey!);
        }
        
        private static string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            for (int i = 0; i < unformattedKey.Length; i++)
            {
                if (i > 0 && i % 4 == 0) result.Append(' ');
                result.Append(unformattedKey[i]);
            }
            return result.ToString().ToUpperInvariant();
        }
        
        private static string GenerateQrCodeUri(string email, string unformattedKey)
        {
            return string.Format(
                "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6",
                Uri.EscapeDataString("HuellitasFelices"),
                Uri.EscapeDataString(email),
                unformattedKey);
        }
    }
}
