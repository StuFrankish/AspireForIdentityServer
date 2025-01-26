using System.Security.Claims;

namespace IdentityServer.Pages.Account.Manage.Models;

public class UserClaimsModel
{
    public List<Claim> Claims { get; set; } = [];
}
