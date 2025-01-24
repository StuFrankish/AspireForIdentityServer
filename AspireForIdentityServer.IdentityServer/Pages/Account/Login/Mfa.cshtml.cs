#nullable disable

using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IdentityServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Pages.Account.Mfa;

[AllowAnonymous]
public class Index(
    SignInManager<ApplicationUser> signInManager,
    IIdentityServerInteractionService interaction,
    ILogger<Index> logger) : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly ILogger<Index> _logger = logger;
    private readonly IIdentityServerInteractionService _interaction = interaction;

    [BindProperty]
    public InputModel Input { get; set; }

    public bool RememberMe { get; set; }
    public string ReturnUrl { get; set; }

    public class InputModel
    {
        [Required]
        [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "Authenticator code")]
        public string TwoFactorCode { get; set; }

        [Display(Name = "Remember this machine")]
        public bool RememberMachine { get; set; }

        public string Button { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(bool rememberMe, string returnUrl = null)
    {
        // Ensure the user has gone through the username & password screen first
        _ = await _signInManager.GetTwoFactorAuthenticationUserAsync() ??
            throw new InvalidOperationException($"Unable to load two-factor authentication user.");

        ReturnUrl = returnUrl;
        RememberMe = rememberMe;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(bool rememberMe, string returnUrl = null)
    {
        var request = await _interaction.GetAuthorizationContextAsync(returnUrl);

        if (Input.Button == "decline")
        {
            if (request is null)
            {
                return Redirect("~/");
            }

            ArgumentNullException.ThrowIfNull(returnUrl, nameof(returnUrl));

            await _interaction.DenyAuthorizationAsync(request, AuthorizationError.AccessDenied);

            if (request.IsNativeClient())
            {
                return this.LoadingPage(returnUrl);
            }

            return Redirect(returnUrl ?? "~/");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        returnUrl ??= Url.Content("~/");

        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync() ??
            throw new InvalidOperationException($"Unable to load two-factor authentication user.");

        var authenticatorCode = Input.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

        var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, Input.RememberMachine);

        if (result.Succeeded)
        {
            _logger.LogInformation("User with ID '{UserId}' logged in with 2fa.", user.Id);
            return LocalRedirect(returnUrl);
        }
        else if (result.IsLockedOut)
        {
            _logger.LogWarning("User with ID '{UserId}' account locked out.", user.Id);
            return RedirectToPage("./Lockout");
        }
        else
        {
            _logger.LogWarning("Invalid authenticator code entered for user with ID '{UserId}'.", user.Id);
            ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
            return Page();
        }
    }

}
