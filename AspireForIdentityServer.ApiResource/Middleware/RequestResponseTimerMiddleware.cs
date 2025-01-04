using System.Diagnostics;

namespace WeatherApi.Middleware;

internal static class RequestResponseTimerMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestResponseTimer(this IApplicationBuilder builder) => builder.UseMiddleware<RequestResponseTimerMiddleware>();
}

internal class RequestResponseTimerMiddleware(RequestDelegate next, ILogger<RequestResponseTimerMiddleware> logger)
{
    private const int MaxMilliseconds = 500;

    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = Stopwatch.GetTimestamp();

        await next(context);

        var endTime = Stopwatch.GetTimestamp();
        var elapsedMilliseconds = Stopwatch.GetElapsedTime(startTime, endTime).Milliseconds;

        if (elapsedMilliseconds > MaxMilliseconds)
        {
            logger.LogWarning("Long Running Request: {Path} - {ElapsedMilliseconds} milliseconds", context.Request.Path, elapsedMilliseconds);
        }
    }
}