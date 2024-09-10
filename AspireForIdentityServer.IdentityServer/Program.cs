using IdentityServer.Extensions;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information(messageTemplate: "Starting IdentityServer Host");

try
{
    var builder = WebApplication
        .CreateBuilder(args);

    builder
        .Host.UseSerilog((ctx, lc) => lc
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
        .Enrich.FromLogContext()
        .ReadFrom.Configuration(ctx.Configuration));

    if (builder.Environment.IsDevelopment())
    {
        // This is a hack to give the database time to start up in development
        Log.Information("Dev Only - Sleeping for 10 seconds");
        Thread.Sleep(10000);
    }

    Log.Information("Configuring & Starting Application...");

    builder
        .AddServiceDefaults();

    builder
        .ConfigureServices()
        .InitializeDatabase()
        .ConfigurePipeline()
        .Run();
}
catch (Exception ex) when (ex.GetType().Name is not "HostAbortedException")
{
    Log.Fatal(ex, messageTemplate: "Unhandled exception");
}
finally
{
    Log.Information(messageTemplate: "Host Shutdown complete");
    Log.CloseAndFlush();
}