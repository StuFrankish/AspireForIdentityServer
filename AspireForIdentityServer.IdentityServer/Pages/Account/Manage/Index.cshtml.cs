using IdentityServer.Models;
using IdentityServer.Pages.Account.Manage.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServer.Pages.Account.Manage;

public class ManageAccountModel(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager) : PageModel
{
    public TwoFactorAuthModel TwoFactorAuthModel { get; set; }
    public UserClaimsModel UserClaimsModel { get; set; }
    public ProfileDataModel ProfileDataModel { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await userManager.GetUserAsync(User);

        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
        }

        await Populate2FAModel(user);
        await PopulateUserClaimsModel(user);
        PopulateProfileDataModel(user);

        return Page();
    }

    private async Task Populate2FAModel(ApplicationUser user)
    {
        TwoFactorAuthModel ??= new TwoFactorAuthModel();

        TwoFactorAuthModel.HasAuthenticator = await userManager.GetAuthenticatorKeyAsync(user) != null;
        TwoFactorAuthModel.Is2faEnabled = await userManager.GetTwoFactorEnabledAsync(user);
        TwoFactorAuthModel.IsMachineRemembered = await signInManager.IsTwoFactorClientRememberedAsync(user);
        TwoFactorAuthModel.RecoveryCodesLeft = await userManager.CountRecoveryCodesAsync(user);
    }

    private void PopulateProfileDataModel(ApplicationUser user)
    {
        ProfileDataModel ??= new ProfileDataModel(user);
    }

    private async Task PopulateUserClaimsModel(ApplicationUser user)
    {
        UserClaimsModel ??= new UserClaimsModel();

        // Add the aspnet user claims to the UserClaimsModel
        UserClaimsModel.Claims = [.. await userManager.GetClaimsAsync(user)];
    }
}
