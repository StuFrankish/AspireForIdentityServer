using IdentityModel;
using Microsoft.AspNetCore.Authorization;
using WeatherApi.Configuration;

namespace WeatherApi.Extensions;

internal static class AuthorizationPolicyExtensions
{
    public static void AddAuthorizationPolicies(this AuthorizationBuilder builder)
    {
        foreach (var config in PolicyConfiguration.AuthorizationPolicies)
        {
            builder.AddPolicy(config.Name, policy =>
            {
                if (config.RequireAuthenticatedUser)
                {
                    policy.RequireAuthenticatedUser();
                }

                if (!config.RequiredScopes.IsNullOrEmpty())
                {
                    policy.RequireClaim(claimType: JwtClaimTypes.Scope, [.. config.RequiredScopes]);
                }
            });
        }
    }
}