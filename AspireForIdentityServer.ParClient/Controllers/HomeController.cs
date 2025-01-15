using Client.Dtos;
using Client.Services;
using Duende.AccessTokenManagement.OpenIdConnect;
using Duende.IdentityModel.Client;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Client.Controllers;

public class HomeController(
    ILogger<HomeController> logger,
    IHttpClientFactory httpClientFactory,
    IUserTokenManagementService userTokenManagementService,
    RedisUserSessionStore redisUserSessionStore,
    IdentityServerSamplesApiService IdentityServerSamplesApiService
) : Controller
{
    private readonly HttpClient _weatherHttpClient = httpClientFactory.CreateClient("WeatherApi");

    [AllowAnonymous]
    public IActionResult Index() => View();

    public async Task<IActionResult> Secure()
    {
        #region Example of using the RedisUserSessionStore

        var hasBeenSecured = redisUserSessionStore.GetString(User.FindFirst("sub").Value, "secure_page");

        if (string.IsNullOrWhiteSpace(hasBeenSecured))
        {
            redisUserSessionStore.SetString(User.FindFirst("sub").Value, "secure_page", "visited");
        }

        var sessionKeys = redisUserSessionStore.GetUserStorageKeys(User.FindFirst("sub").Value);
        var sessionItems = new List<KeyValuePair<string, object>>();

        foreach (var key in sessionKeys)
        {
            var value = redisUserSessionStore.GetString(User.FindFirst("sub").Value, key);
            sessionItems.Add(new KeyValuePair<string, object>(key, value));
        }

        ViewBag.SessionKeyValues = sessionItems;

        #endregion

        #region Example of using the IdentityServerSamplesApiService

        // Call the IdentityServerSamplesApiService to get sample data
        // The endpoint has caching enabled, so the first call will generate new data and cache it.
        var sampleData = await IdentityServerSamplesApiService.GetSampleData();
        ViewBag.SampleData = sampleData;

        #endregion

        #region Using basic Weather API service

        var token = await userTokenManagementService.GetAccessTokenAsync(User);
        _weatherHttpClient.SetBearerToken(token.AccessToken);

        var request = new HttpRequestMessage(HttpMethod.Get, requestUri: "weather/getWeatherForecasts/gb");
        var responseMessage = await _weatherHttpClient.SendAsync(request);

        if (responseMessage.IsSuccessStatusCode)
        {
            string responseBody = await responseMessage.Content.ReadAsStringAsync();
            var serializerOptions = new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true
            };

            var weatherForecasts = JsonSerializer.Deserialize<WeatherForecastResponse>(responseBody, serializerOptions);

            ViewBag.WeatherForecasts = weatherForecasts;
        }

        #endregion

        return View();
    }

    [AllowAnonymous]
    public IActionResult Logout()
    {
        if (User.Identity.IsAuthenticated)
            return SignOut(OpenIdConnectDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction(nameof(LoggedOut));
    }

    [AllowAnonymous]
    public IActionResult LoggedOut()
    {
        if (User.Identity.IsAuthenticated)
        {
            // Redirect to home page if the user is authenticated.
            return RedirectToAction(actionName: "Index", controllerName: "Home");
        }

        return View();
    }

}