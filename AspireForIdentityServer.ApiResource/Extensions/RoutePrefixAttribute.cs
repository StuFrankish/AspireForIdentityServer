namespace WeatherApi.Extensions;

/// <summary>
/// Specifies a route prefix for a given endpoint.
/// </summary>
/// <param name="prefix">The route prefix to apply to the controller.</param>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class RoutePrefixAttribute(string prefix) : Attribute
{
    /// <summary>
    /// Gets the route prefix.
    /// </summary>
    public string Prefix { get; } = prefix;
}
