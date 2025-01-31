using Duende.IdentityServer.EntityFramework.Entities;
using Duende.IdentityServer.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Data.Repositories.Clients;

internal class ClientRepository(IConfigurationDbContext dbContext) : IClientRepository
{
    public async Task<List<Client>> GetClientsWithInitiateLoginUris()
    {
        var clientQuery = dbContext.Clients
            .AsNoTracking()
            .Where(client =>
                client.InitiateLoginUri != null &&
                client.Enabled
            );

        return await clientQuery.ToListAsync();
    }
}