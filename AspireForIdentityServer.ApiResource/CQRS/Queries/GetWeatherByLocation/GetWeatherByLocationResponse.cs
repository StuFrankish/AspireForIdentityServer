using WeatherApi.Entities;

namespace WeatherApi.CQRS.Queries.GetWeatherByLocation;

public class GetWeatherByLocationResponse
{
    public ICollection<WeatherForecast> WeatherForecasts { get; set; } = [];
}
