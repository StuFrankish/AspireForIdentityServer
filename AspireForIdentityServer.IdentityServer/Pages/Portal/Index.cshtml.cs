using Duende.IdentityServer.EntityFramework.Entities;
using IdentityServer.Data.Repositories.Clients;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Pages.Portal;

public class IndexModel(IClientRepository clientRepository) : PageModel
{
    public List<Client> Clients { get; set; }

    public async Task OnGetAsync()
    {
        Clients = await clientRepository.GetClientsWithInitiateLoginUris();
    }
}
