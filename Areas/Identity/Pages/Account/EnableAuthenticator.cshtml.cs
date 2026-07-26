using System.Text;
using HuellitasFelices.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HuellitasFelices.Areas.Identity.Pages.Account
{
    [Authorize]
    public class EnableAuthenticatorModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAuditService _auditService;
        public string SharedKey { get; set; } = string.Empty;
        public string AuthenticatorUri { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }

        public EnableAuthenticatorModel(UserManager<IdentityUser> userManager, IAuditService auditService)
        {
            _userManager = userManager;
            _auditService = auditService;
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
            var valid = await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, code);
            if (!valid)
            {
                ErrorMessage = "Código inválido. Verifique e intente de nuevo.";
                await LoadSharedKeyAndQrCodeUriAsync(user);
                return Page();
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);
            await _auditService.LogAsync("MfaActivado", "IdentityUser", usuarioId: user.Id, usuarioEmail: user.Email,
                direccionIP: HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                valorAnterior: "MFA desactivado", valorNuevo: "MFA activado", descripcion: "El usuario activó MFA.");
            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            TempData["RecoveryCodes"] = recoveryCodes?.ToArray();
            TempData["SuccessMessage"] = "Autenticación multifactor activada correctamente.";
            return RedirectToPage("TwoFactorAuthentication");
        }

        private async Task LoadSharedKeyAndQrCodeUriAsync(IdentityUser user)
        {
            var key = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(key))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                key = await _userManager.GetAuthenticatorKeyAsync(user);
            }
            SharedKey = FormatKey(key!);
            var email = await _userManager.GetEmailAsync(user);
            AuthenticatorUri = GenerateQrCodeUri(email!, key!);
        }

        private static string FormatKey(string key) => string.Concat(key.Select((c, i) => i > 0 && i % 4 == 0 ? " " + c : c.ToString())).ToUpperInvariant();
        private static string GenerateQrCodeUri(string email, string key) => string.Format("otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6", Uri.EscapeDataString("HuellitasFelices"), Uri.EscapeDataString(email), key);
    }
}
