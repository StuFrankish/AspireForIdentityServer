using Asp.Versioning;
using Duende.IdentityServer;
using IdentityServer.Configuration;
using IdentityServer.Extensions.Options;
using IdentityServer.SharedRepositories;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace IdentityServer.Extensions;

internal static class WebApplicationBuilderExtensions
{
    public static void AddAndConfigureIdentityServer(this IHostApplicationBuilder builder)
    {
        var connectionStrings = builder.GetCustomOptionsConfiguration<ConnectionStrings>(ConfigurationSections.ConnectionStrings);

        void ConfigureSqlDbContext(DbContextOptionsBuilder builder) => builder.UseSqlServer(
            connectionString: connectionStrings.SqlServer,
            sql => {
                sql.MigrationsAssembly(typeof(Program).Assembly.GetName().Name);
                sql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
            }
        );

        builder.Services.AddControllers();

        builder.Services.AddIdentityServer(options =>
        {
            // Allow unregistered redirect URIs for PAR clients
            options.PushedAuthorization.AllowUnregisteredPushedRedirectUris = true;

            options.Events.RaiseErrorEvents = true;
            options.Events.RaiseFailureEvents = true;
        })
            .AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = ConfigureSqlDbContext;
                options.EnablePooling = true;
            })
            .AddOperationalStore(options =>
            {
                options.ConfigureDbContext = ConfigureSqlDbContext;
                options.EnablePooling = true;
            })
            .AddServerSideSessions()
            .AddTestUsers(TestUsers.Users);

        // Add optional support for Google authentication
        builder.Services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                    // register your IdentityServer with Google at https://console.developers.google.com
                    // enable the Google+ API
                    // set the redirect URI to https://localhost:5001/signin-google
                    options.ClientId = "copy client ID from Google here";
                    options.ClientSecret = "copy client secret from Google here";
                });

        // Add support for local API authentication
        builder.Services.AddLocalApiAuthentication();

        // Add support for additional shared repositories
        builder.Services.AddScoped<IClientRepository, ClientRepository>();
    }

    public static void AddAndConfigureRedisCache(this IHostApplicationBuilder builder)
    {
        // Get connection strings
        var connectionStrings = builder.GetCustomOptionsConfiguration<ConnectionStrings>(ConfigurationSections.ConnectionStrings);

        // Add Redis Cache
        builder.Services.AddStackExchangeRedisCache(options => {
            options.Configuration = connectionStrings.Redis;
        });
    }

    public static void AddAndConfigureApiVersioning(this WebApplicationBuilder builder)
    {
        builder.Services.AddApiVersioning(options =>
        {
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(majorVersion: 1, minorVersion: 0);
            options.UnsupportedApiVersionStatusCode = (int)System.Net.HttpStatusCode.BadRequest;
        });
    }

    public static void AddAndConfigureDataProtection(this IHostApplicationBuilder builder)
    {
        // Get connection strings  
        var connectionStrings = builder.GetCustomOptionsConfiguration<ConnectionStrings>(ConfigurationSections.ConnectionStrings);

        // Add Data Protection  
        builder.Services.AddDataProtection()
            .SetApplicationName(applicationName: "IdentityServer")
            .PersistKeysToStackExchangeRedis(
                databaseFactory: () => ConnectionMultiplexer.Connect(connectionStrings.Redis).GetDatabase(),
                key: "IdentityServer:DataProtection-Keys"
            );
    }

}