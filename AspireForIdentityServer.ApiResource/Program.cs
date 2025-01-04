using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using WeatherApi.Extensions;
using WeatherApi.Middleware;

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

builder.Services
    .AddAuthorizationBuilder()
    .AddAuthorizationPolicies();

builder.Services
    .AddHealthChecks();

builder.Services
    .AddHttpContextAccessor()
    .AddProblemDetails();

builder.Services.AddStackExchangeRedisCache(o =>
{
    o.Configuration = builder.Configuration.GetConnectionString("Redis");
});

#pragma warning disable EXTEXP0018 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
builder.Services.AddHybridCache();
#pragma warning restore EXTEXP0018


builder.Services.AddMediatR(options =>
{
    options.RegisterServicesFromAssemblies(typeof(Program).Assembly);
});

var app = builder.Build();

app.UseGlobalExceptionHandler();
app.UseRequestResponseTimer();

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
