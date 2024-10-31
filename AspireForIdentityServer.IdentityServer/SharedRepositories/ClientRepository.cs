using Duende.IdentityServer.EntityFramework.Entities;
using Duende.IdentityServer.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer.SharedRepositories;

public class ClientRepository(IConfigurationDbContext dbContext) : IClientRepository
{
    private readonly IConfigurationDbContext _dbContext = dbContext;

    public async Task<List<Client>> GetClientsWithInitiateLoginUris()
    {
        var clientQuery = _dbContext.Clients
            .AsNoTracking()
            .Where(client =>
                client.InitiateLoginUri != null &&
                client.Enabled
            );

        return await clientQuery.ToListAsync();
    }

}