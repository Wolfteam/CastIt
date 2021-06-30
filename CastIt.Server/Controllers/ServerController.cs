using CastIt.Domain.Dtos;
using CastIt.Server.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CastIt.Server.Controllers
{
    public class ServerController : BaseController<ServerController>
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        public ServerController(
            ILogger<ServerController> logger,
            IServerCastService castService,
            IHostApplicationLifetime hostApplicationLifetime)
            : base(logger, castService)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
        }

        [HttpPost("Stop")]
        public IActionResult StopServer()
        {
            _hostApplicationLifetime.StopApplication();
            return Ok(new EmptyResponseDto(true));
        }
    }
}
