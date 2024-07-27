using Client.Common;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using System.Collections.Generic;

namespace Client.Extensions;

public static class ClaimActionsExtensions
{
    /// <summary>
    /// Applies custom claim actions to the claim action collection.
    /// </summary>
    /// <param name="claimActions">The claim action collection to apply the custom claim actions to.</param>
    public static void ApplyCustomClaimsActions(this ClaimActionCollection claimActions)
    {
        claimActions.Clear();
        claimActions.RemoveUnwantedClaimActions();
        claimActions.AddCustomClaimActions();
    }

    private static void AddCustomClaimActions(this ClaimActionCollection claimActions)
    {
        List<string> addClaimActions = [
            JwtClaimTypes.Name,
            JwtClaimTypes.Email,
            JwtClaimTypes.EmailVerified,
            JwtClaimTypes.GivenName,
            JwtClaimTypes.MiddleName,
            JwtClaimTypes.FamilyName,
            JwtClaimTypes.Role
        ];

        addClaimActions.ForEach(claimAction => claimActions.Add(action: new UserClaimAction(claimAction)));
    }

    private static void RemoveUnwantedClaimActions(this ClaimActionCollection claimActions)
    {
        List<string> removeClaimActions = [
            JwtClaimTypes.IdentityProvider,
            JwtClaimTypes.Nonce,
            JwtClaimTypes.AccessTokenHash
        ];

        removeClaimActions.ForEach(claimAction => claimActions.DeleteClaim(claimAction));
    }
}