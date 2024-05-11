using CastIt.Server.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CastIt.Server.Controllers;

[ApiController]
[Route("[controller]")]
public abstract class BaseController : ControllerBase
{
    protected readonly ILogger Logger;
    protected readonly IServerCastService CastService;

    protected BaseController(ILoggerFactory loggerFactory, IServerCastService castService)
    {
        Logger = loggerFactory.CreateLogger(GetType());
        CastService = castService;
    }

    protected void DisableCaching()
    {
        var headers = HttpContext.Response.Headers;
        headers.Append("Expires", "Sat, 26 Jul 1997 05:00:00 GMT");
        //headers.Add("Last-Modified", HttpDate.Format(DateTime.UtcNow));
        headers.Append("Cache-Control", "no-store, no-cache, must-revalidate");
        headers.Append("Pragma", "no-cache");
    }
}