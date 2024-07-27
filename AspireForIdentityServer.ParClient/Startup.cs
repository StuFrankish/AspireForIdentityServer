using Client.Options;
using HealthChecks.UI.Client;
using HealthChecks.Uptime;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;

namespace Client;

public class Startup(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        // Configure our options objects.
        services.Configure<IdentityProviderOptions>(_configuration.GetSection(key: ConfigurationSections.IdentityProvider));

        // Create a local instance of the IDP options for immediate use.
        var identityProviderOptions = new IdentityProviderOptions();
        _configuration.GetSection(ConfigurationSections.IdentityProvider).Bind(identityProviderOptions);

        // Setup the rest of the client.
        services.AddTransient<ParOidcEvents>();
        services.AddSingleton<IDiscoveryCache>(_ => new DiscoveryCache(identityProviderOptions.Authority));

        // add automatic token management
        services.AddOpenIdConnectAccessTokenManagement();

        // Add PAR interaction httpClient
        services.AddHttpClient<ParOidcEvents>(name: "par_interaction_client", options => {
            options.BaseAddress = new Uri(uriString: identityProviderOptions.Authority);
        });

        // Add session
        services.AddDistributedMemoryCache();
        services.AddSession(options =>
        {
            options.Cookie.Name = "mvc.par.session";
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        // add MVC
        services.AddControllersWithViews();

        // add cookie-based session management with OpenID Connect authentication
        services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.Name = "mvc.par";
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
                
                options.GetClaimsFromUserInfoEndpoint = true;
                options.SaveTokens = true;
                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = JwtClaimTypes.Name,
                    RoleClaimType = JwtClaimTypes.Role
                };

            });

        services.AddBff(options => {
            options.EnableSessionCleanup = true;
            options.SessionCleanupInterval = TimeSpan.FromMinutes(5);
        })
        .AddServerSideSessions();

        services.AddHealthChecks()
            .AddUptimeHealthCheck()
            .AddIdentityServer(idSvrUri: new Uri(uriString: identityProviderOptions.Authority));
    }

    public void Configure(IApplicationBuilder app, IHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();
        app.UseSession();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseHealthChecks(path: "/_health", options: new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            AllowCachingResponses = false
        });

        app.UseEndpoints(endpoints =>
        {
            endpoints
                .MapDefaultControllerRoute()
                .RequireAuthorization();

            endpoints
                .MapBffManagementEndpoints();
        });
    }
}