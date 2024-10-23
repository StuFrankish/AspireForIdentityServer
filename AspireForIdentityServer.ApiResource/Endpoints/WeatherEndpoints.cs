using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using WeatherApi.Entities;
using WeatherApi.Extensions;

namespace WeatherApi.Endpoints;

public class WeatherEndpoints : EndpointGroupBase
{
    private readonly DistributedCacheEntryOptions _defaultCacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    };

    public override void Map(WebApplication app)
    {
        // Map the group of endpoints to the application
        var configuration = app.MapGroup(this);

        // Add authorization policy for the whole group
        configuration.RequireAuthorization(PolicyNames.WeatherReader);

        // Map the endpoints to the group
        configuration
            .MapGet(pattern: "/getWeatherForecasts/{locale}", GetWeatherForecastsAsync);
    }

    public async Task<IEnumerable<WeatherForecast>> GetWeatherForecastsAsync(
        string locale,
        ILogger<WeatherEndpoints> logger,
        IDistributedCache distributedCache,
        CancellationToken cancellationToken
    )
    {
        logger.LogInformation(message: "Getting Weather Forecast");

        string[] Summaries =
        [
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        ];

        WeatherForecast[] weatherForecasts;
        string cacheKey = $"weather_forecast_data_{locale}";

        // Check if the data is in the cache
        var cachedData = await distributedCache.GetStringAsync(
            key: cacheKey,
            token: cancellationToken
        );

        if (cachedData is null)
        {
            logger.LogInformation(message: "Weather Forecast not found in cache");

            // Service to fetch weather data
            weatherForecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();

            // Serialize and cache the data
            cachedData = JsonSerializer.Serialize(weatherForecasts);
            await distributedCache.SetStringAsync(
                key: cacheKey,
                value: cachedData,
                options: _defaultCacheOptions,
                token: cancellationToken
            );
        }
        else
        {
            logger.LogInformation(message: "Weather Forecast found in cache");
            weatherForecasts = JsonSerializer.Deserialize<WeatherForecast[]>(cachedData) ?? [];
        }

        return weatherForecasts!;
    }

}