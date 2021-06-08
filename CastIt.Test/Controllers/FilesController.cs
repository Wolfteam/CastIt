using System.Threading.Tasks;
using CastIt.Domain.Dtos;
using CastIt.Test.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CastIt.Test.Controllers
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
