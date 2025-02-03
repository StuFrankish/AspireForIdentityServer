using Asp.Versioning;
using Duende.IdentityServer;
using Duende.IdentityServer.EntityFramework.DbContexts;
using IdentityServer.Configuration;
using IdentityServer.Data.DbContexts;
using IdentityServer.Data.Entities.Identity;
using IdentityServer.Data.Repositories.Clients;
using IdentityServer.Data.Repositories.Users;
using IdentityServer.Extensions.Options;
using IdentityServer.SqlInterceptors;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace IdentityServer.Extensions;

internal static class WebApplicationBuilderExtensions
{
    public static void AddAndConfigureSqlServer(this IHostApplicationBuilder builder)
    {
        // Register the interceptor in the DI container
        builder.Services.AddSingleton<ICustomInterceptor, CustomCommandInterceptor>();

        // Add DbContexts
        builder.Services.AddDbContext<ConfigurationDbContext>(configuredSqlOptions());
        builder.Services.AddDbContext<PersistedGrantDbContext>(configuredSqlOptions());
        builder.Services.AddDbContext<ApplicationDbContext>(configuredSqlOptions());

        // Configure DbContext options
        static Action<IServiceProvider, DbContextOptionsBuilder> configuredSqlOptions() => (serviceProvider, optionsBuilder) =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            var connectionStrings = configuration.GetSection(ConfigurationSections.ConnectionStrings).Get<ConnectionStrings>();
            var databaseSettings = configuration.GetSection(ConfigurationSections.DatabaseSettings).Get<DatabaseSettings>();

            optionsBuilder
                .UseSqlServer(
                    connectionString: $"{connectionStrings.SqlServer};Pooling=true;Min Pool Size={databaseSettings.MinPoolSize};Max Pool Size={databaseSettings.MaxPoolSize}",
                    sql =>
                    {
                        sql.MigrationsAssembly(typeof(Program).Assembly.GetName().Name);
                        sql.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null
                        );
                    })

                // Resolve the interceptors and add them to the optionsBuilder
                .AddInterceptors([.. serviceProvider.GetServices<ICustomInterceptor>()]);
        };

        // Add support for additional shared repositories
        builder.Services.AddScoped<IClientRepository, ClientRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
    }

    public static void AddAndConfigureIdentityServer(this IHostApplicationBuilder builder)
    {
        builder.Services.AddControllers();

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // Configure Application Cookie settings
        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.AccessDeniedPath = "/home/accessdenied";
        });

        builder.Services.AddIdentityServer(options =>
        {
            // Configure Cookie
            options.Authentication.CookieLifetime = TimeSpan.FromHours(8);
            options.Authentication.CookieSlidingExpiration = true;

            // Allow unregistered redirect URIs for PAR clients
            options.PushedAuthorization.AllowUnregisteredPushedRedirectUris = true;

            // Events
            options.Events.RaiseErrorEvents = true;
            options.Events.RaiseFailureEvents = true;
        })
            .AddConfigurationStore()
            .AddOperationalStore()
            .AddServerSideSessions()
            .AddAspNetIdentity<ApplicationUser>();

        // Add optional support for Google authentication
        builder.Services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.ClientId = "copy client ID from Google here";
                    options.ClientSecret = "copy client secret from Google here";
                    options.CallbackPath = "/signin-google";

                    options.Events.OnRedirectToAuthorizationEndpoint = context =>
                    {
                        context.Response.Redirect(context.RedirectUri + "&prompt=select_account");
                        return Task.CompletedTask;
                    };
                });

        // Add optional support for Microsoft authentication
        builder.Services.AddAuthentication()
                .AddMicrosoftAccount(options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.ClientId = "copy client ID from Microsoft here";
                    options.ClientSecret = "copy client secret from Microsoft here";
                });

        // Add support for local API authentication
        builder.Services.AddLocalApiAuthentication();
    }

    public static void AddAndConfigurePolicyAuthorization(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthorizationBuilder()
            .AddPolicy("UserAdmin", policy =>
            {
                policy.RequireRole("user.admin");
            });
    }

    public static void AddAndConfigureRedisCache(this IHostApplicationBuilder builder)
    {
        // Get connection strings
        var connectionStrings = builder.GetCustomOptionsConfiguration<ConnectionStrings>(ConfigurationSections.ConnectionStrings);

        // Add Redis Cache
        builder.Services.AddStackExchangeRedisCache(options =>
        {
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

    public static void AddAndConfigureFido2Services(this IHostApplicationBuilder builder)
    {
        builder.Services.AddFido2(options =>
        {
            options.Origins = builder.Configuration.GetSection("Fido2:Origins").Get<HashSet<string>>();
            options.ServerDomain = builder.Configuration["Fido2:ServerDomain"];
            options.ServerName = builder.Configuration["Fido2:ServerName"];
            options.TimestampDriftTolerance = builder.Configuration.GetValue<int>("Fido2:TimestampDriftTolerance");
        });

        builder.Services.AddSession(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
        });
    }

}