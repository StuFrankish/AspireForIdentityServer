using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

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
    .AddPolicy("WeatherReader", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "Weather.Read");
    });

builder.Services.AddHealthChecks();

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

app.MapControllers()
    .RequireAuthorization();

app.Run();
