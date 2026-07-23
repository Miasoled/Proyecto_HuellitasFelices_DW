using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HuellitasFelices.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterConfirmationModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string Email { get; set; } = string.Empty;

        public void OnGet()
        {
            if (string.IsNullOrEmpty(Email))
            {
                Email = "";
            }
        }
    }
}
