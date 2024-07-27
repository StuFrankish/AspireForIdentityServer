using Client.Dtos;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Client.Services;

public class IdentityServerSamplesApiService(IdentityServerApiServiceBase serviceBase)
{
    public async Task<List<SampleDto>> GetSampleData()
    {
        string apiPath = $"samples/getsample";
        return await serviceBase.CallApi<List<SampleDto>>(HttpMethod.Get, apiPath) ?? [];
    }
}