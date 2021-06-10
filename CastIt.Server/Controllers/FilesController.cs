using CastIt.Domain.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using CastIt.Server.Interfaces;

namespace CastIt.Server.Controllers
{
    public class FilesController : BaseController<FilesController>
    {
        public FilesController(
            ILogger<FilesController> logger,
            IServerCastService castService)
            : base(logger, castService)
        {
        }

        [HttpPost("{fileId}/[action]")]
        public async Task<IActionResult> Play(long fileId)
        {
            await CastService.PlayFile(fileId, true, false);
            return Ok(new EmptyResponseDto(true));
        }
    }
}
