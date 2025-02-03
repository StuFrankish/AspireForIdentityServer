using Duende.IdentityServer.EntityFramework.Entities;

namespace IdentityServer.Data.Repositories.Clients;

public interface IClientRepository
{
    Task<List<Client>> GetClientsWithInitiateLoginUris();
}
