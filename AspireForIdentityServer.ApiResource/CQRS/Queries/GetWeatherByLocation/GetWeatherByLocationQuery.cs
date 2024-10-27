using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using WeatherApi.Entities;

namespace WeatherApi.CQRS.Queries.GetWeatherByLocation;

public class GetWeatherByLocationQuery : IRequest<GetWeatherByLocationResponse>
{
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
        // Start the request response
        _logger.LogInformation(message: "Getting Weather Forecast");
        string cacheKey = $"weather_forecast_data_{request.Location}";
        var responseObject = new GetWeatherByLocationResponse();

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

}