using Duende.IdentityServer.Events;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using IdentityServer.Data.Models.Account;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Pages.Account.Manage;

public class SecurityModel(
    IIdentityServerInteractionService interactionService,
    IClientStore clientStore,
    IResourceStore resourceStore,
    IEventService eventService) : PageModel
{
    public AccountGrantsModel View { get; set; }

    public async Task OnGet()
    {
        var grants = await interactionService.GetAllUserGrantsAsync();

        var list = new List<GrantViewModel>();
        foreach (var grant in grants)
        {
            var client = await clientStore.FindClientByIdAsync(grant.ClientId);
            if (client != null)
            {
                var resources = await resourceStore.FindResourcesByScopeAsync(grant.Scopes);

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

        View = new AccountGrantsModel
        {
            Grants = list
        };
    }

    [BindProperty]
    [Required]
    public string ClientId { get; set; }

    public async Task<IActionResult> OnPost()
    {
        await interactionService.RevokeUserConsentAsync(ClientId);
        await eventService.RaiseAsync(new GrantsRevokedEvent(User.GetSubjectId(), ClientId));

        return RedirectToPage("/Grants/Index");
    }
}
