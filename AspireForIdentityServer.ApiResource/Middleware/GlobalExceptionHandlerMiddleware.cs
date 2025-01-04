using Microsoft.AspNetCore.Mvc;

namespace WeatherApi.Middleware;

internal static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder) => builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
}

internal class GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger, IProblemDetailsService problemDetailsService)
{
    private readonly Dictionary<Type, Func<HttpContext, Exception, ILogger<GlobalExceptionHandlerMiddleware>, ProblemDetails>> _configuredHandlers = new()
    {
        { typeof(NotImplementedException), HandleNotImplementedException },
        { typeof(KeyNotFoundException), HandleKeyNotFoundException }
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            context.Response.ContentType = "application/json";
            ProblemDetails problemDetails;

            if (_configuredHandlers.TryGetValue(ex.GetType(), out var handler))
            {
                logger.LogInformation("Running configured error handler for '{ExceptionType}'", ex.GetType().Name);

                problemDetails = handler.Invoke(context, ex, logger);

                await WriteProblemDetails(context, problemDetails, ex);
                return;
            }

            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            problemDetails = HandleUnhandledException(context, ex, logger);

            await WriteProblemDetails(context, problemDetails, ex);
        }
    }

    private async Task WriteProblemDetails(HttpContext context, ProblemDetails problemDetails, Exception exception)
    {
        context.Response.StatusCode = problemDetails.Status!.Value;
        await problemDetailsService.WriteAsync(new() { HttpContext = context, Exception = exception, ProblemDetails = problemDetails });
    }

    private static ProblemDetails HandleNotImplementedException(HttpContext httpContext, Exception exception, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        var problemDetails = new ProblemDetails
        {
            Title = "The requested functionality is not implemented.",
            Status = StatusCodes.Status501NotImplemented,
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        };

        logger.LogCritical(exception, "Not implemented exception: {Message}", exception.Message);

        return problemDetails;
    }
    private static ProblemDetails HandleKeyNotFoundException(HttpContext httpContext, Exception exception, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        var problemDetails = new ProblemDetails
        {
            Title = "The requested resource was not found.",
            Status = StatusCodes.Status404NotFound,
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        };

        logger.LogWarning(exception, "Key not found exception: {Message}", exception.Message);

        return problemDetails;
    }
    private static ProblemDetails HandleUnhandledException(HttpContext httpContext, Exception exception, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        var problemDetails = new ProblemDetails
        {
            Title = "An error occured, please try again later.",
            Status = StatusCodes.Status500InternalServerError,
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        };

        logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        return problemDetails;
    }
}