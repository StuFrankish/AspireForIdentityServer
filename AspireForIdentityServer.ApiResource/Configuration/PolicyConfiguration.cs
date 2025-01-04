namespace WeatherApi.Configuration;

internal static class PolicyConfiguration
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

internal class PolicyConfig
{
    public required string Name { get; set; }
    public List<string> RequiredScopes { get; set; } = [];
    public bool RequireAuthenticatedUser { get; set; } = true;
}

internal struct PolicyNames
{
    public const string WeatherReader = "WeatherReader";
    public const string WeatherWriter = "WeatherWriter";
}