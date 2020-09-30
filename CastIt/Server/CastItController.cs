using CastIt.Common.Utils;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using System.IO;
using System.Threading.Tasks;

namespace CastIt.Server
{
    public class CastItController : WebApiController
    {
        [Route(HttpVerbs.Get, "/image")]
        public async Task GetImgUrl([QueryField] string filePath, [QueryField] long tentativeSeconds)
        {
            var path = FileUtils.GetClosestThumbnail(filePath, tentativeSeconds);

            if (!FileUtils.Exists(path))
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
