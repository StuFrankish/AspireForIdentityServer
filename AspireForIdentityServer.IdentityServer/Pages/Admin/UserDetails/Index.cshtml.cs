using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Pages.Admin.UserDetails;

[Authorize(policy: "Admin")]
public class IndexModel : PageModel
{
    [FromRoute]
    public Guid Id { get; set; }

    public void OnGet()
    {

    }
}
