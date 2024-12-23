using FluentAssertions;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using System.Text.Json;
using WeatherApi.CQRS.Queries.GetWeatherByLocation;
using WeatherApi.Entities;

namespace AspireForIdentityServer.Tests.WeatherApi;

public class WeatherAPISampleTests
{
    [Theory (Skip = "HybridCache not fully supported")]
    [InlineData("gb")]
    [InlineData("eu")]
    public async Task GetWeatherForecastQuery_Returns_AppropriateDataResponse(string locale)
    {
        // Arrange
        var mockLogger = new Mock<ILogger<GetWeatherByLocationQuery>>();
        var mockHybridCache = new Mock<HybridCache>();

        string[] Summaries =
        [
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        ];

        // Mock data for the weather forecast
        var mockWeatherData = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = index,
            Summary = Summaries[index]
        }).ToList();

        // Convert the mock data to JSON
        var mockWeatherDataBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(mockWeatherData));

        // Mock the HybridCache.GetOrCreateAsync method
        mockHybridCache
            .Setup(x =>
                x.GetOrCreateAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<CancellationToken, ValueTask<List<WeatherForecast>>>>(),
                    null,
                    null,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(mockWeatherData);

        // Setup the Weather API Mediator Request
        var request = new GetWeatherByLocationQuery(locale);
        var handler = new GetWeatherByLocationHandler(
            hybridCache: mockHybridCache.Object,
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
