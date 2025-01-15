using Duende.AccessTokenManagement.OpenIdConnect;
using Duende.IdentityModel.Client;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Client.Services;

public class IdentityServerApiServiceBase(
    IUserTokenManagementService userTokenManagementService,
    IHttpContextAccessor contextAccessor,
    HttpClient httpClient
)
{
    private readonly JsonSerializerOptions _defaultJsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<T> CallApi<T>(HttpMethod httpMethod, string endpoint, object payload = null)
    {
        var response = await ExecuteApiCall(httpMethod, endpoint, payload);
        string responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return default;

        return JsonSerializer.Deserialize<T>(responseBody, _defaultJsonSerializerOptions) ?? default!;
    }

    public async Task<bool> CallApi(HttpMethod httpMethod, string endpoint, object payload = null, HttpStatusCode expectation = HttpStatusCode.OK)
    {
        var response = await ExecuteApiCall(httpMethod, endpoint, payload);
        return response.StatusCode == expectation;
    }

    private async Task<HttpResponseMessage> ExecuteApiCall(HttpMethod method, string requesturl, object requestContent = null)
    {
        var token = await userTokenManagementService.GetAccessTokenAsync(contextAccessor!.HttpContext!.User);
        httpClient.SetBearerToken(token.AccessToken ?? string.Empty);

        var request = new HttpRequestMessage(method, requesturl);
        if (requestContent != null)
        {
            request.Content = JsonContent.Create(requestContent);
        }

        return await httpClient.SendAsync(request);
    }
}