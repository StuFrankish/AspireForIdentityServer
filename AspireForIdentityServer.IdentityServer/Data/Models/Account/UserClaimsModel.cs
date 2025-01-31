using System.Security.Claims;

namespace IdentityServer.Data.Models.Account;

public class UserClaimsModel
{
    public List<Claim> Claims { get; set; } = [];
}
