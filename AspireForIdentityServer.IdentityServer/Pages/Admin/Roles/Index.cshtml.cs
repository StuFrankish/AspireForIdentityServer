using IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IdentityServer.Pages.Admin.Roles;

public class IndexModel(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager
) : PageModel
{
    public async Task OnGet()
    {
        var user = await userManager.GetUserAsync(User);
        
        var createResult = await roleManager.CreateAsync(new IdentityRole("superuser"));

        if (createResult.Succeeded)
        {
            var role = await roleManager.FindByNameAsync("superuser");
            await roleManager.AddClaimAsync(role, new Claim("permission", "admin"));

            await userManager.AddToRoleAsync(user, role.ToString());
        }

        var allRoles = await roleManager.Roles.ToListAsync(CancellationToken.None);
        var userClaims = await userManager.GetClaimsAsync(user);

    }
}
