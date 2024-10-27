using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
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

    public async Task<Results<Ok<ICollection<WeatherForecast>>, ProblemHttpResult>> GetWeatherForecastsAsync(string locale, IMediator mediator, CancellationToken cancellationToken)
    {
        try
        {
            // Get the weather forecast by location
            var queryResponse = await mediator.Send(request: new GetWeatherByLocationQuery { Location = locale }, cancellationToken);
            return TypedResults.Ok(queryResponse.WeatherForecasts);
        }
        catch (Exception ex)
        {
            // Return a problem response
            return TypedResults.Problem(ex.Message);
        }
    }
}