using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VideoStreaming.Models;

namespace VideoStreaming.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }
    public IActionResult Error()
    {
        var feature = HttpContext.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();

        ViewBag.Path = feature?.Path;
        ViewBag.Message = feature?.Error.Message;
        ViewBag.StackTrace = feature?.Error.StackTrace;
        return View();
    }

}
