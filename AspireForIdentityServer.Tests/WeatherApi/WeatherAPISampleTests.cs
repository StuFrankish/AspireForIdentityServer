using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using System.Text.Json;
using WeatherApi.CQRS.Queries.GetWeatherByLocation;
using WeatherApi.Entities;

namespace AspireForIdentityServer.Tests.WeatherApi;

public class WeatherAPISampleTests
{
    [Theory]
    [InlineData("gb")]
    [InlineData("eu")]
    public async Task GetWeatherForecastQuery_Returns_AppropriateDataResponse(string locale)
    {
        // Arrange
        var mockLogger = new Mock<ILogger<GetWeatherByLocationQuery>>();
        var mockDistributedCache = new Mock<IDistributedCache>();

        string[] Summaries =
        [
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        ];

        mockDistributedCache
            .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = index,
                    Summary = Summaries[index]
                }).ToList()
            )));

        // Setup the Weather API Mediator Request
        var request = new GetWeatherByLocationQuery { Location = locale };
        var handler = new GetWeatherByLocationHandler(
            distributedCache: mockDistributedCache.Object,
            logger: mockLogger.Object
        );

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();

        if (locale == "gb")
        {
            response.WeatherForecasts.Should().NotBeNullOrEmpty();
        }
        else
        {
            response.WeatherForecasts.Should().BeNullOrEmpty();
        }

    }
}
