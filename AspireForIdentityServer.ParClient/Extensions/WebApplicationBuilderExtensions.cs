using Client.Options;
using Client.Services;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Polly;
using StackExchange.Redis;
using System;

namespace Client.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static void AddAndConfigurePushedAuthorizationSupport(this WebApplicationBuilder builder)
    {
        var identityProviderOptions = builder.GetCustomOptionsConfiguration<IdentityProviderOptions>(ConfigurationSections.IdentityProvider);

        // Register the IdentityProviderOptions for DI
        builder.Services.Configure<IdentityProviderOptions>(builder.Configuration.GetSection(ConfigurationSections.IdentityProvider));

        // Setup the rest of the client.
        builder.Services.AddTransient<ParOidcEvents>();
        builder.Services.AddSingleton<IDiscoveryCache>(_ => new DiscoveryCache(identityProviderOptions.Authority));

        // Add PAR interaction httpClient
        builder.Services.AddHttpClient<ParOidcEvents>(name: "par_interaction_client", options =>
        {
            options.BaseAddress = new Uri(uriString: identityProviderOptions.Authority);
        });
    }

    public static void AddAndConfigureRemoteApi(this WebApplicationBuilder builder)
    {
        var identityProviderOptions = builder.GetCustomOptionsConfiguration<IdentityProviderOptions>(ConfigurationSections.IdentityProvider);
        var weatherApiOptions = builder.GetCustomOptionsConfiguration<WeatherApiOptions>(ConfigurationSections.WeatherApi);
        var resilienceOptions = builder.GetCustomOptionsConfiguration<PollyResilienceOptions>(ConfigurationSections.PollyResilience);

        // add automatic token management
        builder.Services.AddOpenIdConnectAccessTokenManagement();

        // Add IdentityServerApi httpClient
        builder.Services.AddHttpClient(name: nameof(IdentityServerApiServiceBase), options =>
        {
            options.BaseAddress = new Uri(uriString: identityProviderOptions.Authority + "/api/v1/");
        })
            .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(retryCount: resilienceOptions.AllowedRetryCountBeforeFailure, retryAttempt => TimeSpan.FromMilliseconds(100 * Math.Pow(x: 2, retryAttempt))))
            .AddTransientHttpErrorPolicy(policy => policy.CircuitBreakerAsync(resilienceOptions.AllowedEventsBeforeCircuitBreaker, TimeSpan.FromSeconds(value: resilienceOptions.DurationOfBreakSeconds)))
            .AddUserAccessTokenHandler();

        // Add Weather httpClient
        builder.Services.AddHttpClient(name: "WeatherApi", options =>
        {
            options.BaseAddress = new Uri(uriString: $"{weatherApiOptions.BaseUrl}/api/");
        })
            .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(retryCount: resilienceOptions.AllowedRetryCountBeforeFailure, retryAttempt => TimeSpan.FromMilliseconds(100 * Math.Pow(x: 2, retryAttempt))))
            .AddTransientHttpErrorPolicy(policy => policy.CircuitBreakerAsync(resilienceOptions.AllowedEventsBeforeCircuitBreaker, TimeSpan.FromSeconds(value: resilienceOptions.DurationOfBreakSeconds)))
            .AddUserAccessTokenHandler();
    }

    public static void AddAndConfigureSessionWithRedis(this WebApplicationBuilder builder)
    {
        var connectionStrings = builder.GetCustomOptionsConfiguration<ConnectionStringsOptions>(ConfigurationSections.ConnectionStrings);
        var identityProviderOptions = builder.GetCustomOptionsConfiguration<IdentityProviderOptions>(ConfigurationSections.IdentityProvider);

        // Add session
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.Name = $"{identityProviderOptions.ClientId}_session";
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
            ConnectionMultiplexer.Connect(connectionStrings.Redis)
        );

        builder.Services.AddStackExchangeRedisCache(o =>
        {
            o.Configuration = connectionStrings.Redis;
        });

        builder.Services.AddBff()
            .AddServerSideSessions<RedisUserSessionStore>();

        // Register the RedisUserSessionStore
        builder.Services.AddSingleton<RedisUserSessionStore>();
    }

    public static void AddAndConfigureAuthorization(this WebApplicationBuilder builder)
    {
        var identityProviderOptions = builder.GetCustomOptionsConfiguration<IdentityProviderOptions>(ConfigurationSections.IdentityProvider);

        // add cookie-based session management with OpenID Connect authentication
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.Name = $"{identityProviderOptions.ClientId}_app";
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

                options.AccessDeniedPath = "/Error/AccessDenied";

                options.Events.OnSigningOut = async e =>
                {
                    // automatically revoke refresh token at signout time
                    await e.HttpContext.RevokeRefreshTokenAsync();
                };

            })
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                // Needed to add PAR support
                options.EventsType = typeof(ParOidcEvents);

                // Setup Client
                options.Authority = identityProviderOptions.Authority;
                options.ClientId = identityProviderOptions.ClientId;
                options.ClientSecret = identityProviderOptions.ClientSecret;

                // code flow + PKCE (PKCE is turned on by default and required by the identity provider in this sample)
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.UsePkce = true;

                options.Scope.Clear();
                options.Scope.Add(OpenIdConnectScope.OpenId);
                options.Scope.Add(OpenIdConnectScope.OfflineAccess);
                options.Scope.Add("profile");
                options.Scope.Add("IdentityServerApi");
                options.Scope.Add("Weather.Read");

                options.ClaimActions.ApplyCustomClaimsActions();

                options.GetClaimsFromUserInfoEndpoint = true;
                options.SaveTokens = true;
                options.MapInboundClaims = false;
                options.DisableTelemetry = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = JwtClaimTypes.Name,
                    RoleClaimType = JwtClaimTypes.Role
                };

            });
    }
}
