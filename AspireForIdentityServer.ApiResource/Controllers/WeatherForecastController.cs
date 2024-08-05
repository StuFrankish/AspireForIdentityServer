using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using WeatherApi.Entities;

namespace WeatherApi.Controllers;

[ApiController]
[Authorize(policy: "WeatherReader")]
[Route("[controller]")]
public class WeatherForecastController(ILogger<WeatherForecastController> logger, IDistributedCache distributedCache) : ControllerBase
{
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    private readonly IDistributedCache _distributedCache = distributedCache;
    private readonly ILogger<WeatherForecastController> _logger = logger;

    private readonly DistributedCacheEntryOptions _defaultCacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    };

    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IEnumerable<WeatherForecast>> Get()
    {
        _logger.LogInformation(message: "Getting Weather Forecast");

        WeatherForecast[] weatherForecasts;
        string cacheKey = "weather_forecast_data";

        // Check if the data is in the cache
        var cachedData = await _distributedCache.GetStringAsync(
            key: cacheKey,
            token: _cancellationToken
        );

        if (cachedData is null)
        {
            _logger.LogInformation(message: "Weather Forecast not found in cache");

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
            await _distributedCache.SetStringAsync(
                key: cacheKey,
                value: cachedData,
                options: _defaultCacheOptions,
                token: _cancellationToken
            );
        }
        else
        {
            _logger.LogInformation(message: "Weather Forecast found in cache");
            weatherForecasts = JsonSerializer.Deserialize<WeatherForecast[]>(cachedData);
        }

        return weatherForecasts!;
    }
}
