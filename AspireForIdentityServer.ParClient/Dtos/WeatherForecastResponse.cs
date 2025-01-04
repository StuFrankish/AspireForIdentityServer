using System.Collections.Generic;

namespace Client.Dtos;

public class WeatherForecastResponse
{
    public IEnumerable<WeatherForecastDto> WeatherForecasts { get; set; } = [];
}
