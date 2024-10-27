using MediatR;
using WeatherApi.CQRS.Queries.GetWeatherByLocation;
using WeatherApi.Entities;
using WeatherApi.Extensions;

namespace WeatherApi.Endpoints;

[RoutePrefix("/weather")]
public class WeatherEndpoints : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        // Map the group of endpoints to the application
        var routeGroupBuilder = app.MapGroup(this);

        // Add authorization policy for the whole group
        routeGroupBuilder.RequireAuthorization(PolicyNames.WeatherReader);

        // Map the endpoints with their handlers
        routeGroupBuilder
            .MapGet(pattern: "/getWeatherForecasts/{locale}", GetWeatherForecastsAsync);
    }

    public async Task<IEnumerable<WeatherForecast>> GetWeatherForecastsAsync(string locale, IMediator mediator, CancellationToken cancellationToken)
    {
        var queryResponse = await mediator.Send(new GetWeatherByLocationQuery { Location = locale });
        return queryResponse.WeatherForecasts;
    }
}