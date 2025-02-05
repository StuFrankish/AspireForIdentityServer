using Duende.IdentityServer.Events;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Pages.Account.Grants;

[Authorize]
public class Index(IIdentityServerInteractionService interaction,
    IClientStore clients,
    IResourceStore resources,
    IEventService events) : PageModel
{
    private readonly IIdentityServerInteractionService _interaction = interaction;
    private readonly IClientStore _clients = clients;
    private readonly IResourceStore _resources = resources;
    private readonly IEventService _events = events;

    public ViewModel View { get; set; }

    public async Task OnGet()
    {
        var grants = await _interaction.GetAllUserGrantsAsync();

        var list = new List<GrantViewModel>();
        foreach (var grant in grants)
        {
            var client = await _clients.FindClientByIdAsync(grant.ClientId);
            if (client != null)
            {
                var resources = await _resources.FindResourcesByScopeAsync(grant.Scopes);

                var item = new GrantViewModel()
                {
                    ClientId = client.ClientId,
                    ClientName = client.ClientName ?? client.ClientId,
                    ClientLogoUrl = client.LogoUri,
                    ClientUrl = client.ClientUri,
                    Description = grant.Description,
                    Created = grant.CreationTime,
                    Expires = grant.Expiration,
                    IdentityGrantNames = [.. resources.IdentityResources.Select(x => x.DisplayName ?? x.Name)],
                    ApiGrantNames = [.. resources.ApiScopes.Select(x => x.DisplayName ?? x.Name)]
                };

                list.Add(item);
            }
        }

        View = new ViewModel
        {
            Grants = list
        };
    }

    [BindProperty]
    [Required]
    public string ClientId { get; set; }

    public async Task<IActionResult> OnPost()
    {
        await _interaction.RevokeUserConsentAsync(ClientId);
        await _events.RaiseAsync(new GrantsRevokedEvent(User.GetSubjectId(), ClientId));

        return RedirectToPage("/Grants/Index");
    }
}