using IdentityServer.Data.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Pages.Admin.Roles;

public class IndexModel(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager
) : PageModel
{
    public async Task OnGet()
    {
        ViewData["ActivePage"] = "Roles";
    }
}
