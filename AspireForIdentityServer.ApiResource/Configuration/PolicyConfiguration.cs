using WeatherApi.Extensions;

namespace WeatherApi.Configuration;

public static class PolicyConfiguration
{
    public static List<PolicyConfig> AuthorizationPolicies =>
    [
        new() {
            Name = PolicyNames.WeatherReader,
            RequireAuthenticatedUser = true,
            RequiredScopes = ["Weather.Read"]
        },
        new() {
            Name = PolicyNames.WeatherWriter,
            RequireAuthenticatedUser = true,
            RequiredScopes = ["Weather.Write"]
        }
    ];
}