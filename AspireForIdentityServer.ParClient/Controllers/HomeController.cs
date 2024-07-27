using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Client.Controllers;

public class HomeController() : Controller
{
    [AllowAnonymous]
    public IActionResult Index() => View();

    public IActionResult Secure() => View();

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