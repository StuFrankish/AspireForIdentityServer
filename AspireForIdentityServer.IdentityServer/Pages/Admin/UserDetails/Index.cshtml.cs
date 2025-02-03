using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Pages.Admin.UserDetails;

[Authorize(policy: "UserAdmin")]
public class IndexModel : PageModel
{
    [FromRoute]
    public Guid Id { get; set; }

    public void OnGet()
    {
        ViewData["ActivePage"] = "Users";
    }
}
