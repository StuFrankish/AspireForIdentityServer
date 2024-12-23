using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using WeatherApi.Entities;

namespace WeatherApi.CQRS.Queries.GetWeatherByLocation;

/// <summary>
/// Query to get weather information by location.
/// </summary>
public class GetWeatherByLocationQuery(string location) : IRequest<GetWeatherByLocationResponse>
{
    /// <summary>
    /// Gets or sets the location for which to retrieve the weather.
    /// </summary>
    public string Location { get; } = location;
}

public class GetWeatherByLocationHandler(HybridCache hybridCache,ILogger<GetWeatherByLocationQuery> logger)
    : IRequestHandler<GetWeatherByLocationQuery, GetWeatherByLocationResponse>
{
    public async Task<GetWeatherByLocationResponse> Handle(GetWeatherByLocationQuery request,CancellationToken cancellationToken)
    {
        // Create the response object
        var responseObject = new GetWeatherByLocationResponse();

        // Start the request response
        logger.LogInformation(message: "Getting Weather Forecast");

        // Ensure the locale is valid
        var requestedLocale = EnsureValidLocale(request.Location, logger);

        // If the locale is invalid, return an empty response
        if (string.IsNullOrWhiteSpace(requestedLocale)) return responseObject;

        // Get the data from cache
        responseObject.WeatherForecasts = await hybridCache.GetOrCreateAsync(
            key: $"weather_forecast_data_{requestedLocale}",
            async cancelToken => await getMockedDataAsync(cancelToken),
            cancellationToken: cancellationToken
        );

        return responseObject;

        // Here for demo purposes
        async Task<List<WeatherForecast>> getMockedDataAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation(message: "Weather Forecast not found in cache");

            string[] Summaries =
            [
                "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            ];

            return [.. Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })];
        }
    }

    /// <summary>
    /// Ensures the provided locale is valid.
    /// </summary>
    /// <param name="locale"></param>
    /// <param name="logger"></param>
    /// <exception cref="ArgumentException"></exception>
    private static string EnsureValidLocale(string locale, ILogger logger)
    {
        // Ensure the provided string is not null or empty
        if (string.IsNullOrWhiteSpace(locale))
        {
            logger.LogError(message: "Invalid locale - Empty value");
            throw new ArgumentException(message: "Invalid locale", paramName: nameof(locale));
        }

        // Get a list of valid locales
        var validLocales = new[] { "gb", "fr" };

        // Ensure the provided locale is valid
        if (!validLocales.Contains(locale))
        {
            logger.LogError(message: "Invalid locale - Not found in valid list");
        }

        // Return the locale from the list of valid locales if it is valid
        return validLocales.FirstOrDefault(l => l == locale) ?? string.Empty;
    }

}