using Client.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Client.Extensions;

public static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        // Add custom services
        builder.AddAndConfigureRemoteApi();
        builder.AddAndConfigureSessionWithRedis();
        builder.AddAndConfigureAuthorization();

        // Add DI Services
        builder.Services.AddScoped<IdentityServerSamplesApiService>();
        builder.Services.AddTransient<IdentityServerApiServiceBase>();
        builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        // Add MVC
        builder.Services.AddControllersWithViews();

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseSession();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapDefaultControllerRoute()
            .RequireAuthorization();

        app.MapBffManagementBackchannelEndpoint();

        return app;
    }
}
