using IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Pages.Admin.Users;

public class IndexModel(UserManager<ApplicationUser> userManager) : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public List<ApplicationUser> Users { get; set; }

    public async Task OnGetAsync()
    {
        Users = await Task.Run(() => _userManager.Users.ToList());
    }
}
