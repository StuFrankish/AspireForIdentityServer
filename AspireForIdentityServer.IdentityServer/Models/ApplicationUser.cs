using IdentityServer.Models.Fido;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Models;

public class ApplicationUser : IdentityUser
{
    public ICollection<PublicKeyCredential> PublicKeyCredentials { get; set; } = [];
}