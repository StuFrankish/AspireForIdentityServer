using IdentityServer.Data.Entities.Fido;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Data.Entities.Identity;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;

    public ICollection<PublicKeyCredential> PublicKeyCredentials { get; set; } = [];
}