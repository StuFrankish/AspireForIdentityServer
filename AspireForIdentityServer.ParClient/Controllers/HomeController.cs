using Client.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client.Controllers;

public class HomeController(
    ILogger<HomeController> logger,
    RedisUserSessionStore redisUserSessionStore,
    IdentityServerSamplesApiService IdentityServerSamplesApiService
) : Controller
{
    private readonly ILogger<HomeController> _logger = logger;

    private readonly IdentityServerSamplesApiService _identityServerSamplesApiService = IdentityServerSamplesApiService;
    private readonly RedisUserSessionStore _redisUserSessionStore = redisUserSessionStore;

    [AllowAnonymous]
    public IActionResult Index() => View();

    public async Task<IActionResult> Secure()
    {
        #region Example of using the RedisUserSessionStore

        var hasBeenSecured = _redisUserSessionStore.GetString(User.FindFirst("sub").Value, "secure_page");

        if (string.IsNullOrWhiteSpace(hasBeenSecured))
        {
            _redisUserSessionStore.SetString(User.FindFirst("sub").Value, "secure_page", "visited");
        }

        var sessionKeys = _redisUserSessionStore.GetUserStorageKeys(User.FindFirst("sub").Value);
        var sessionItems = new List<KeyValuePair<string, object>>();

        foreach (var key in sessionKeys)
        {
            var value = _redisUserSessionStore.GetString(User.FindFirst("sub").Value, key);
            sessionItems.Add(new KeyValuePair<string, object>(key, value));
        }

        ViewBag.SessionKeyValues = sessionItems;

        #endregion

        #region Example of using the IdentityServerSamplesApiService

        // Call the IdentityServerSamplesApiService to get sample data
        // The endpoint has caching enabled, so the first call will generate new data and cache it.
        var sampleData = await _identityServerSamplesApiService.GetSampleData();
        ViewBag.SampleData = sampleData;

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