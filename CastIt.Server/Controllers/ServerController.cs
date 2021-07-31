using CastIt.Domain.Dtos;
using CastIt.Server.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Mime;
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

        /// <summary>
        /// Stops the server gracefully
        /// </summary>
        /// <returns>Returns the result of the operation</returns>
        [HttpPost("Stop")]
        [ProducesResponseType(typeof(EmptyResponseDto), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> StopServer()
        {
            await _serverService.StopAsync();
            return Ok(new EmptyResponseDto(true));
        }
    }
}
