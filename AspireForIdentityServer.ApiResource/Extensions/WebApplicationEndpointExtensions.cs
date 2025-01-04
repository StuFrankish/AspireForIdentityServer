using System.Reflection;

namespace WeatherApi.Extensions;

internal static class WebApplicationEndpointExtensions
{
    /// <summary>
    /// Maps the specified endpoint group and applies a computed route prefix
    /// </summary>
    /// <param name="webApplication"></param>
    /// <param name="endpointGroup"></param>
    /// <returns></returns>
    public static RouteGroupBuilder MapGroup(this WebApplication webApplication, IEndpointGroup endpointGroup)
    {
        // Retrieve the RoutePrefix attribute from the endpoint group
        var routePrefixAttribute = endpointGroup.GetType().GetCustomAttribute<RoutePrefixAttribute>();
        var prefix = routePrefixAttribute?.Prefix ?? endpointGroup.GetType().Name;

        // Map the group of endpoints to the application
        var routeGroupBuilder = webApplication.MapGroup(prefix: $"/api/{prefix.TrimStart(trimChar: '/')}");

        // Return the route group builder
        return routeGroupBuilder;
    }

    /// <summary>
    /// Finds and maps all valid endpoints that derive from EndpointGroupBase
    /// </summary>
    /// <param name="webApplication"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static WebApplication MapEndpoints(this WebApplication webApplication)
    {
        var assembly = Assembly.GetExecutingAssembly();

        var endpointGroupTypes = assembly.GetExportedTypes()
            .Where(type =>
                typeof(IEndpointGroup).IsAssignableFrom(type) && !type.IsAbstract
            );

        foreach (var endpointGroupType in endpointGroupTypes)
        {
            _ = endpointGroupType.GetConstructor(Type.EmptyTypes) ?? throw new InvalidOperationException(
                message: $"Endpoint group {endpointGroupType.Name} must have a parameterless constructor"
            );

            if (Activator.CreateInstance(endpointGroupType) is IEndpointGroup instance)
            {
                instance.Map(webApplication);
            }
        }

        return webApplication;
    }
}
