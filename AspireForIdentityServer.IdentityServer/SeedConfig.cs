using Duende.IdentityServer.Models;

namespace IdentityServer;

public static class SeedConfig
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        [
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        ];

    public static IEnumerable<ApiScope> ApiScopes =>
        [
            new ApiScope("api"),
            new ApiScope("IdentityServerApi", "Identity Server API")
        ];

    public static IEnumerable<Client> Clients =>
        [
            // Push Authorization Request (PAR) client
            new Client
            {
                ClientId = "mvc.par",
                ClientName = "MVC PAR Client",

                ClientSecrets =
                {
                    new Secret("secret".Sha256())
                },

                CoordinateLifetimeWithUserSession = true,

                RequireConsent = true,
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,

                RequirePushedAuthorization = true,

                BackChannelLogoutSessionRequired = true,
                BackChannelLogoutUri = "https://localhost:5002/bff/backchannel",
                PostLogoutRedirectUris = { "https://localhost:5002/signout-callback-oidc" },

                AllowOfflineAccess = true,

                AllowedScopes = { "openid", "profile", "api", "IdentityServerApi" }
            }
        ];
}
