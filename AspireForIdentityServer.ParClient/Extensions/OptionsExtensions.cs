using Client.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using System;

namespace Client.Extensions;

public static class OptionsExtensions
{
    /// <summary>
    /// Retrieves a custom configuration section from an application's configuration and binds it to a strongly typed object.
    /// </summary>
    /// <typeparam name="T">The type of the custom options class to bind to, which must implement the <see cref="ICustomOptions"/> interface and have a parameterless constructor.</typeparam>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/> instance used to build the web application.</param>
    /// <param name="configurationSectionName">The name of the configuration section in the application's configuration files (e.g., appsettings.json) that contains the settings to be bound to the custom options class.</param>
    /// <returns>An instance of the specified custom options class <typeparamref name="T"/>, with its properties populated from the specified configuration section.</returns>
    /// <remarks>
    /// This method creates a new instance of the custom options class and uses the <see cref="IConfiguration"/> available in <paramref name="builder"/> to bind the configuration values from the specified section to the properties of the class.
    /// </remarks>
    public static T GetCustomOptionsConfiguration<T>(this WebApplicationBuilder builder, string configurationSectionName) where T : class, ICustomOptions, new()
    {
        var optionsClass = Activator.CreateInstance<T>();
        builder.Configuration.GetSection(configurationSectionName).Bind(optionsClass);

        return optionsClass;
    }
}