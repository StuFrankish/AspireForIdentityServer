using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Client.Controllers;

public class ErrorController() : Controller
{
    [HttpGet, AllowAnonymous]
    public IActionResult AuthError()
    {
        var errorMessage = HttpContext.Session.GetString("ErrorMessage");
        ViewBag.ErrorMessage = errorMessage;

        if (string.IsNullOrWhiteSpace(errorMessage))
            return RedirectToAction("Index", "Home");

        HttpContext.Session.Remove("AuthError");

        return View();
    }

    [HttpGet, AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
