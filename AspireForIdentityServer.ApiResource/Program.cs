using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using WeatherApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration.GetValue<string>("IdentityProvider:Authority");
        options.TokenValidationParameters.ValidateAudience = false;
        options.Validate();
    });

builder.Services.AddAuthorizationBuilder()
    .AddAuthorizationPolicies();

builder.Services.AddHealthChecks();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddStackExchangeRedisCache(o =>
{
    o.Configuration = builder.Configuration.GetConnectionString("Redis");
});



var app = builder.Build();

app.MapDefaultEndpoints();

app.UseHttpsRedirection();

app.UseHealthChecks(path: "/_health", options: new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
    AllowCachingResponses = false
});

app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();

app.Run();
