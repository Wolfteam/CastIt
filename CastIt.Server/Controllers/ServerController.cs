using CastIt.Domain.Dtos;
using CastIt.Server.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CastIt.Server.Controllers
{
    public class ServerController : BaseController<ServerController>
    {
        private readonly IServerService _serverService;
        public ServerController(
            ILogger<ServerController> logger,
            IServerCastService castService,
            IServerService serverService)
            : base(logger, castService)
        {
            _serverService = serverService;
        }

        [HttpPost("Stop")]
        public async Task<IActionResult> StopServer()
        {
            await _serverService.StopAsync();
            return Ok(new EmptyResponseDto(true));
        }
    }
}
