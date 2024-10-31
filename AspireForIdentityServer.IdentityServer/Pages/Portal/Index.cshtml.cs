using Duende.IdentityServer.EntityFramework.Entities;
using IdentityServer.SharedRepositories;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Pages.Portal;

public class IndexModel(IClientRepository clientRepository) : PageModel
{
    private readonly IClientRepository _clientRepository = clientRepository ?? throw new ArgumentException(message: "Client Repository cannot be null", paramName: nameof(clientRepository));

    public List<Client> Clients { get; set; }

    public async Task OnGetAsync()
    {
        Clients = await _clientRepository.GetClientsWithInitiateLoginUris();
    }
}
