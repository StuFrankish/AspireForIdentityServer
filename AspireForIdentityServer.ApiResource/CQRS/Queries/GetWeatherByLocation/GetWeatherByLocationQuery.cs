using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using WeatherApi.Entities;

namespace WeatherApi.CQRS.Queries.GetWeatherByLocation;

/// <summary>
/// Query to get weather information by location.
/// </summary>
public class GetWeatherByLocationQuery : IRequest<GetWeatherByLocationResponse>
{
    /// <summary>
    /// Gets or sets the location for which to retrieve the weather.
    /// </summary>
    public string Location { get; set; } = string.Empty;
}

public class GetWeatherByLocationHandler(
    IDistributedCache distributedCache,
    ILogger<GetWeatherByLocationQuery> logger
    ) : IRequestHandler<GetWeatherByLocationQuery, GetWeatherByLocationResponse>
{
    private readonly IDistributedCache _distributedCache = distributedCache;
    private readonly ILogger<GetWeatherByLocationQuery> _logger = logger;

    private readonly DistributedCacheEntryOptions _defaultCacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    };

    public async Task<GetWeatherByLocationResponse> Handle(
        GetWeatherByLocationQuery request,
        CancellationToken cancellationToken
    )
    {
        // Create the response object
        var responseObject = new GetWeatherByLocationResponse();

        // Start the request response
        _logger.LogInformation(message: "Getting Weather Forecast");

        // Ensure the locale is valid
        var requestedLocale = EnsureValidLocale(request.Location, _logger);

        // If the locale is invalid, return an empty response
        if (string.IsNullOrWhiteSpace(requestedLocale)) return responseObject;

        // Cache key for the weather forecast data
        string cacheKey = $"weather_forecast_data_{requestedLocale}";
        
        // Check if the data is in the cache
        var cachedData = await _distributedCache.GetStringAsync(
            key: cacheKey,
            token: cancellationToken
        );

        string[] Summaries =
        [
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        ];

        if (cachedData is null)
        {
            _logger.LogInformation(message: "Weather Forecast not found in cache");

            // Service to fetch weather data
            responseObject.WeatherForecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();

            // Serialize and cache the data
            cachedData = JsonSerializer.Serialize(responseObject.WeatherForecasts);
            await _distributedCache.SetStringAsync(
                key: cacheKey,
                value: cachedData,
                options: _defaultCacheOptions,
                token: cancellationToken
            );
        }
        else
        {
            _logger.LogInformation(message: "Weather Forecast found in cache");
            responseObject.WeatherForecasts = JsonSerializer.Deserialize<WeatherForecast[]>(cachedData) ?? [];
        }

        return responseObject;
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