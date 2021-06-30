using CastIt.Server.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CastIt.Server.Controllers
{
    //TODO: SWAGGER DOC XD ?
    [ApiController]
    [Route("[controller]")]
    public abstract class BaseController<T> : ControllerBase
    {
        protected readonly ILogger<T> Logger;
        protected readonly IServerCastService CastService;

        protected BaseController(ILogger<T> logger, IServerCastService castService)
        {
            Logger = logger;
            CastService = castService;
        }

        protected void DisableCaching()
        {
            var headers = HttpContext.Response.Headers;
            headers.Add("Expires", "Sat, 26 Jul 1997 05:00:00 GMT");
            //headers.Add("Last-Modified", HttpDate.Format(DateTime.UtcNow));
            headers.Add("Cache-Control", "no-store, no-cache, must-revalidate");
            headers.Add("Pragma", "no-cache");
        }
    }
}
