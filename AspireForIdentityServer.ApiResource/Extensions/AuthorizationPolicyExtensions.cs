using Microsoft.AspNetCore.Authorization;
using WeatherApi.Configuration;

namespace WeatherApi.Extensions;

public static class AuthorizationPolicyExtensions
{
    public static void AddAuthorizationPolicies(this AuthorizationBuilder builder)
    {
        const string setClaimType = "scope";

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
                    policy.RequireClaim(claimType: setClaimType, [.. config.RequiredScopes]);
                }
            });
        }
    }
}