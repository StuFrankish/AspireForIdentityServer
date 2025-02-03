using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IdentityServer.Data.Entities.Identity;
using IdentityServer.Extensions;
using IdentityServer.Pages.Create;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Pages.Account.Create;

[SecurityHeaders]
[AllowAnonymous]
public class Index(
    IIdentityServerInteractionService interaction,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signinManager) : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    private readonly SignInManager<ApplicationUser> _signInManager = signinManager ?? throw new ArgumentNullException(nameof(signinManager));
    private readonly IIdentityServerInteractionService _interaction = interaction ?? throw new ArgumentNullException(nameof(interaction));

    [BindProperty]
    public InputModel Input { get; set; }

    public IActionResult OnGet(string returnUrl)
    {
        Input = new InputModel { ReturnUrl = returnUrl };
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var context = await _interaction.GetAuthorizationContextAsync(Input.ReturnUrl);

        // Handle the cancel button click
        if (Input.Button != "create")
        {
            if (context != null)
            {
                await _interaction.DenyAuthorizationAsync(context, AuthorizationError.AccessDenied);

                if (context.IsNativeClient())
                {
                    return this.LoadingPage(Input.ReturnUrl);
                }

                return Redirect(Input.ReturnUrl);
            }
            return Redirect("~/");
        }

        // Check if user already exists
        var existingUser = await _userManager.FindByNameAsync(Input.Username);
        if (existingUser != null)
        {
            ModelState.AddModelError("Input.Username", "Username is already taken.");
            return Page();
        }

        // Create the user
        var userToCreate = new ApplicationUser
        {
            UserName = Input.Username,
            Email = Input.Email
        };

        var createResult = await _userManager.CreateAsync(userToCreate, Input.Password);

        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }

        await _userManager.AddClaimAsync(userToCreate, new System.Security.Claims.Claim("display_name", Input.Name));

        await _signInManager.SignInAsync(userToCreate, false);

        // Handle return URL logic
        if (context != null)
        {
            if (context.IsNativeClient())
            {
                return this.LoadingPage(Input.ReturnUrl);
            }

            return Redirect(Input.ReturnUrl);
        }

        if (Url.IsLocalUrl(Input.ReturnUrl))
        {
            return Redirect(Input.ReturnUrl);
        }
        else if (string.IsNullOrEmpty(Input.ReturnUrl))
        {
            return Redirect("~/");
        }
        else
        {
            throw new Exception("Invalid return URL");
        }
    }
}
