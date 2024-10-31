using Duende.IdentityServer.EntityFramework.Entities;

namespace IdentityServer.SharedRepositories;

public interface IClientRepository
{
    Task<List<Client>> GetClientsWithInitiateLoginUris();
}
