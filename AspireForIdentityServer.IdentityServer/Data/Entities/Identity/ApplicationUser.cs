using IdentityServer.Data.Entities.Fido;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Data.Entities.Identity;

public class ApplicationUser : IdentityUser
{
    public ICollection<PublicKeyCredential> PublicKeyCredentials { get; set; } = [];
}