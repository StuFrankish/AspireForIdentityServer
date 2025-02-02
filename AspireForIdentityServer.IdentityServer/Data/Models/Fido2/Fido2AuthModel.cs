using IdentityServer.Data.Entities.Fido;

namespace IdentityServer.Data.Models.Fido2;

public class Fido2AuthModel
{
    public int RegisteredCredentialsCount { get; set; } = 0;
    public List<PublicKeyCredential> PublicKeyCredentials { get; set; } = [];
}
