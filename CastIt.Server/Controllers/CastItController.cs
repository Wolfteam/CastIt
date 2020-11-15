using CastIt.Application.Interfaces;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using System.IO;
using System.Threading.Tasks;

namespace CastIt.Server.Controllers
{
    public class CastItController : WebApiController
    {
        private readonly IFileService _fileService;

        public CastItController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [Route(HttpVerbs.Get, "/image")]
        public async Task GetImgUrl([QueryField] string filePath, [QueryField] long tentativeSeconds)
        {
            var path = _fileService.GetClosestThumbnail(filePath, tentativeSeconds);

            if (!_fileService.Exists(path))
            {
                HttpContext.Response.StatusCode = 400;
                return;
            }

            await using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

            HttpContext.Response.ContentType = "image/jpeg";
            await using var stream = HttpContext.OpenResponseStream();
            await fs.CopyToAsync(stream).ConfigureAwait(false);
        }
    }
}
