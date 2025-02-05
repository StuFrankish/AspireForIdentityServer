using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Pages.Device;

[Authorize]
public class SuccessModel : PageModel
{
    public void OnGet()
    {
    }
}