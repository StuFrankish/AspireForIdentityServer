using IdentityServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Pages.Admin.Users;

[Authorize(policy: "Superuser")]
public class IndexModel(UserManager<ApplicationUser> userManager) : PageModel
{
    public List<ApplicationUser> Users { get; set; }

    public async Task OnGetAsync()
    {
        Users = await Task.Run(() => userManager.Users.ToList());
    }
}
