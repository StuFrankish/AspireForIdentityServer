using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using IdentityServer.Data.DbContexts;
using IdentityServer.Data.Entities.Identity;
using IdentityServer.Handlers;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace IdentityServer.Extensions;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorPages()
            .AddRazorRuntimeCompilation();

        builder.AddAndConfigureSqlServer();
        builder.AddAndConfigureRedisCache();
        builder.AddAndConfigureIdentityServer();
        builder.AddAndConfigureFido2Services();
        builder.AddAndConfigurePolicyAuthorization();
        builder.AddAndConfigureApiVersioning();
        builder.AddAndConfigureDataProtection();

        return builder.Build();
    }

    public static WebApplication MigrateAndSeedDatabase(this WebApplication app)
    {
        if (app.Environment.IsProduction()) return app;

        // Create a usable service scope
        using var serviceScope = app.Services.GetService<IServiceScopeFactory>().CreateScope();

        // Create a list of contexts to loop through and migrate.
        List<Type> contexts = [
            typeof(PersistedGrantDbContext),
            typeof(ConfigurationDbContext),
            typeof(ApplicationDbContext)
        ];

        foreach (var context in contexts)
        {
            var dbContext = (DbContext)serviceScope.ServiceProvider.GetRequiredService(context);
            dbContext.Database.Migrate();
        }

        // Create the seedinig configuration from JSON
        var seedConfig = new SeedConfig("SeedingConfig.json");
        bool saveChanges = false;

        // Create an instance of the Configuration & Identity Db Contexts
        var configurationDbContext = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
        var applicationDbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Seed clients
        if (!configurationDbContext.Clients.Any())
        {
            foreach (var client in seedConfig.Clients)
            {
                configurationDbContext.Clients.Add(client.ToEntity());
            }

            saveChanges = true;
        }

        // Seed resources
        if (!configurationDbContext.IdentityResources.Any())
        {
            foreach (var resource in seedConfig.IdentityResources)
            {
                configurationDbContext.IdentityResources.Add(resource.ToEntity());
            }

            saveChanges = true;
        }

        // Seed API Scopes
        if (!configurationDbContext.ApiScopes.Any())
        {
            foreach (var resource in seedConfig.ApiScopes)
            {
                configurationDbContext.ApiScopes.Add(resource.ToEntity());
            }

            saveChanges = true;
        }

        // Seed users
        if (!applicationDbContext.Users.Any())
        {
            var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            roleManager.CreateAsync(new IdentityRole("user.admin"))
                .GetAwaiter().GetResult();

            foreach (var seedUser in seedConfig.Users)
            {
                var userToCreate = new ApplicationUser
                {
                    UserName = seedUser.Username,
                    Email = seedUser.Email
                };

                var createdUser = userManager.CreateAsync(user: userToCreate, password: seedUser.Password)
                    .GetAwaiter().GetResult();

                userManager.AddClaimAsync(userToCreate, new("display_name", seedUser.DisplayName))
                    .GetAwaiter().GetResult();

                if (seedUser.IsAdmin)
                {
                    userManager.AddToRoleAsync(userToCreate, "user.admin")
                        .GetAwaiter().GetResult();
                }
            }

            saveChanges = true;
        }

        // Save changes if needed
        if (saveChanges) configurationDbContext.SaveChanges();

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

        app.UseSession();

        app.MapRazorPages()
            .RequireAuthorization();

        // Map the FIDO2 routes
        app.MapPost("/fido2/createattestation", Fido2Handler.CreateAttestationAsync);
        app.MapPost("/fido2/createattestationoptions", Fido2Handler.CreateAttestationOptionsAsync);
        app.MapPost("/fido2/createassertion", Fido2Handler.CreateAssertionAsync);
        app.MapPost("/fido2/createassertionoptions", Fido2Handler.CreateAssertionOptions);

        app.MapControllers();

        return app;
    }

}