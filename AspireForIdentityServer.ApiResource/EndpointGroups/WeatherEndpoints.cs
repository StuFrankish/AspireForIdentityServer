using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using WeatherApi.Configuration;
using WeatherApi.CQRS.Queries.GetWeatherByLocation;
using WeatherApi.Extensions;

namespace WeatherApi.EndpointGroups;

[RoutePrefix("/weather")]
public class WeatherEndpoints : IEndpointGroup
{
    public void Map(WebApplication app)
    {
        // Map the group of endpoints to the application
        var routeGroupBuilder = app.MapGroup(this);

        // Add authorization policy for the whole group
        routeGroupBuilder.RequireAuthorization(PolicyNames.WeatherReader);

        // Map the endpoints with their handlers
        routeGroupBuilder
            .MapGet(pattern: "/getWeatherForecasts/{locale}", GetWeatherForecastsAsync);
    }

    public async Task<Results<Ok<GetWeatherByLocationResponse>, ProblemHttpResult>> GetWeatherForecastsAsync(string locale, IMediator mediator, CancellationToken cancellationToken)
    {
        // Get the weather forecast by location
        var response = await mediator.Send(new GetWeatherByLocationQuery(locale), cancellationToken);
        return TypedResults.Ok(response);
    }

}