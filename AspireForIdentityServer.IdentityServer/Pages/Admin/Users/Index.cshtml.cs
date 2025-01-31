using IdentityServer.Data.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Pages.Admin.Users;

[Authorize(policy: "UserAdmin")]
public class IndexModel(UserManager<ApplicationUser> userManager) : PageModel
{
    public List<ApplicationUser> Users { get; set; }

    public async Task OnGetAsync()
    {
        ViewData["ActivePage"] = "Users"; // Ensure this is set
        Users = await Task.Run(() => userManager.Users.ToList());
    }
}
