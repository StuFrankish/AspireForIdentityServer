using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace IdentityServer.Extensions;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorPages();
        builder.AddAndConfigureRedisCache();
        builder.AddAndConfigureIdentityServer();
        builder.AddAndConfigureApiVersioning();

        return builder.Build();
    }

    public static WebApplication InitializeDatabase(this WebApplication app)
    {
        // Create a usable service scope
        using var serviceScope = app.Services.GetService<IServiceScopeFactory>().CreateScope();

        // Create a list of contexts to loop through and migrate.
        List<Type> contexts = [
            typeof(PersistedGrantDbContext),
            typeof(ConfigurationDbContext)
        ];

        foreach (var context in contexts)
        {
            var dbContext = (DbContext)serviceScope.ServiceProvider.GetRequiredService(context);
            dbContext.Database.Migrate();
        }

        // If in Development, run test data seeding
        if (app.Environment.IsDevelopment())
        {
            // Create an instance of the ConfigurationDbContext so we can seed data.
            var configurationContext = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();

            // Seed clients
            if (!configurationContext.Clients.Any())
            {
                foreach (var client in SeedConfig.Clients)
                {
                    configurationContext.Clients.Add(client.ToEntity());
                }
                configurationContext.SaveChanges();
            }

            // Seed resources
            if (!configurationContext.IdentityResources.Any())
            {
                foreach (var resource in SeedConfig.IdentityResources)
                {
                    configurationContext.IdentityResources.Add(resource.ToEntity());
                }
                configurationContext.SaveChanges();
            }

            // Seed API Scopes
            if (!configurationContext.ApiScopes.Any())
            {
                foreach (var resource in SeedConfig.ApiScopes)
                {
                    configurationContext.ApiScopes.Add(resource.ToEntity());
                }
                configurationContext.SaveChanges();
            }

        }

        return app;
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        bool isDevEnvironment = app.Environment.IsDevelopment();

        app.UseSerilogRequestLogging();

        if (isDevEnvironment)
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseForwardedHeaders(new ForwardedHeadersOptions {
            ForwardedHeaders = ForwardedHeaders.All
        });

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();

        app.UseIdentityServer();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapRazorPages()
            .RequireAuthorization();

        app.MapControllers();

        return app;
    }

}