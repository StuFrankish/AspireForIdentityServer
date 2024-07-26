using Asp.Versioning;
using IdentityServer.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using static Duende.IdentityServer.IdentityServerConstants;

namespace IdentityServer.ApiControllers;

[ApiVersion(version: 1)]
[Authorize(LocalApi.PolicyName)]
[ApiController, Route(template: "api/v{v:apiVersion}/samples")]
public class SampleApiController(IDistributedCache distributedCache, ILogger<SampleApiController> logger, CancellationToken cancellationToken)
    : ApiControllerBase
{
    private readonly CancellationToken _cancellationToken = cancellationToken;
    private readonly IDistributedCache _distributedCache = distributedCache;
    private readonly ILogger<SampleApiController> _logger = logger;

    [HttpGet, Route(template: "getsample")]
    [MapToApiVersion(version: 1)]
    public async Task<IEnumerable<SampleDto>> GetSampleCollectionV1()
    {
        // Advisory Note:
        // This is a sample method to demonstrate cached responses, this is NOT a best practice.
        // Caching should be done in a service layer, not in a controller.

        _logger.LogInformation("GetSampleCollectionV1 called");
        List<SampleDto> dataToReturn;

        var cacheKey = "querycache:sampledata";
        var cachedData = await _distributedCache.GetStringAsync(cacheKey, _cancellationToken);

        if (cachedData is null)
        {
            _logger.LogInformation("SampleCollection not found in cache");

            dataToReturn =
            [
                new SampleDto { Id = 1, Name = "Sample 1" },
                new SampleDto { Id = 2, Name = "Sample 2" }
            ];

            cachedData = JsonSerializer.Serialize(dataToReturn);
            await _distributedCache.SetStringAsync(cacheKey, cachedData, _defaultCacheOptions, _cancellationToken);
        }
        else
        {
            _logger.LogInformation("SampleCollection found in cache");
            dataToReturn = JsonSerializer.Deserialize<List<SampleDto>>(cachedData);
        }

        return dataToReturn;
    }
}
