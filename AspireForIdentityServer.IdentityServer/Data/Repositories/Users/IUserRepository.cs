using IdentityServer.Data.Entities.Fido;

namespace IdentityServer.Data.Repositories.Users;

public interface IUserRepository
{
    Task<List<PublicKeyCredential>> GetUserPublicKeyCredentialsAsync(string userId);
}
