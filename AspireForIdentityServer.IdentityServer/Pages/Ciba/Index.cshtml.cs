// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Pages.Ciba;

[AllowAnonymous]
public class IndexModel(IBackchannelAuthenticationInteractionService backchannelAuthenticationInteractionService, ILogger<IndexModel> logger) : PageModel
{
    public BackchannelUserLoginRequest LoginRequest { get; set; }

    private readonly IBackchannelAuthenticationInteractionService _backchannelAuthenticationInteraction = backchannelAuthenticationInteractionService;
    private readonly ILogger<IndexModel> _logger = logger;

    public async Task<IActionResult> OnGet(string id)
    {
        LoginRequest = await _backchannelAuthenticationInteraction.GetLoginRequestByInternalIdAsync(id);
        if (LoginRequest == null)
        {
            _logger.LogWarning("Invalid backchannel login id {id}", id);
            return RedirectToPage("/Home/Error/Index");
        }

        return Page();
    }
}