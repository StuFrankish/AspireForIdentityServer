using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using IdentityServer.Data.Entities.Identity;
using IdentityServer.Extensions;

namespace IdentityServer.Pages.ExternalLogin;

[AllowAnonymous]
[SecurityHeaders]
public class Callback(
    IIdentityServerInteractionService interaction,
    IEventService events,
    ILogger<Callback> logger,
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager) : PageModel
{
    private readonly IIdentityServerInteractionService _interaction = interaction;
    private readonly IEventService _events = events;
    private readonly ILogger<Callback> _logger = logger;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<IActionResult> OnGetAsync()
    {
        // Read external identity from the temporary cookie
        var result = await HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
        if (result?.Succeeded != true)
        {
            throw new Exception("External authentication error");
        }

        var externalUser = result.Principal;

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            var externalClaims = externalUser.Claims.Select(c => $"{c.Type}: {c.Value}");
            _logger.LogDebug("External claims: {@claims}", externalClaims);
        }

        // Determine the unique ID of the external user
        var userIdClaim = externalUser.FindFirst(ClaimTypes.NameIdentifier) ??
                          externalUser.FindFirst("sub") ?? // OIDC subject claim
                          throw new Exception("Unknown userid");

        var provider = result.Properties.Items["scheme"];
        var providerUserId = userIdClaim.Value;

        // Find the existing user by external login info
        var user = await _userManager.FindByLoginAsync(provider, providerUserId);
        if (user == null)
        {
            // Register new user if not found (auto-provisioning example)
            user = new ApplicationUser
            {
                UserName = providerUserId,
                Email = externalUser.FindFirst(ClaimTypes.Email)?.Value
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                throw new Exception("Failed to create new user.");
            }

            // Link external login to the new user
            var addLoginResult = await _userManager.AddLoginAsync(user, new UserLoginInfo(provider, providerUserId, provider));
            if (!addLoginResult.Succeeded)
            {
                throw new Exception("Failed to link external login.");
            }
        }

        // Sign in the user with additional claims
        var additionalLocalClaims = new List<Claim>();
        var localSignInProps = new AuthenticationProperties();
        CaptureExternalLoginContext(result, additionalLocalClaims, localSignInProps);

        await _signInManager.SignInAsync(user, localSignInProps.IsPersistent);

        // Remove the temporary cookie used during external authentication
        await HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);

        // Retrieve return URL
        var returnUrl = result.Properties.Items["returnUrl"] ?? "~/";

        // Check if external login is in the context of an OIDC request
        var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
        await _events.RaiseAsync(new UserLoginSuccessEvent(provider, providerUserId, user.Id, user.UserName, true, context?.Client.ClientId));

        if (context != null && context.IsNativeClient())
        {
            return this.LoadingPage(returnUrl);
        }

        return Redirect(returnUrl);
    }

    // Capture external login context (OIDC signout support)
    private static void CaptureExternalLoginContext(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
    {
        var sid = externalResult.Principal.Claims.FirstOrDefault(x => x.Type == "sid");
        if (sid != null)
        {
            localClaims.Add(new Claim("sid", sid.Value));
        }

        var idToken = externalResult.Properties.GetTokenValue("id_token");
        if (idToken != null)
        {
            localSignInProps.StoreTokens([new AuthenticationToken { Name = "id_token", Value = idToken }]);
        }
    }
}
