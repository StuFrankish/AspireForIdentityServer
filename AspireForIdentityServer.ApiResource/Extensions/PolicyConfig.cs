namespace WeatherApi.Extensions;

public class PolicyConfig
{
    public required string Name { get; set; }
    public List<string> RequiredScopes { get; set; } = [];
    public bool RequireAuthenticatedUser { get; set; } = true;
}

public struct PolicyNames
{
    public const string WeatherReader = "WeatherReader";
    public const string WeatherWriter = "WeatherWriter";
}