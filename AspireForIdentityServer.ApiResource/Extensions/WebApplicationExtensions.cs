using System.Reflection;

namespace WeatherApi.Extensions;

public static class WebApplicationExtensions
{
    public static RouteGroupBuilder MapGroup(this WebApplication webApplication, EndpointGroupBase endpointGroup)
    {
        return webApplication.MapGroup(prefix: $"/api/{endpointGroup.GetType().Name}");
    }

    public static WebApplication MapEndpoints(this WebApplication webApplication)
    {
        var assembly = Assembly.GetExecutingAssembly();

        var endpointGroupTypes = assembly.GetExportedTypes()
            .Where(t => t.IsSubclassOf(typeof(EndpointGroupBase)));

        foreach (var endpointGroupType in endpointGroupTypes)
        {
            if (Activator.CreateInstance(endpointGroupType) is EndpointGroupBase instance)
            {
                instance.Map(webApplication);
            }
        }

        return webApplication;
    }
}
