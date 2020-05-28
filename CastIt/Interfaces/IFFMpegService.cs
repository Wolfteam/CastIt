using EmbedIO;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CastIt.Interfaces
{
    public interface IFFMpegService
    {
        string GetThumbnail(string mrl, int second);
        void GenerateThumbmnails(string mrl);
        void KillThumbnailProcess();
        Task<double> GetFileDuration(string mrl, CancellationToken token);
        void KillTranscodeProcess();
        Task TranscodeVideo(Stream outputStream, string filePath, double seconds, CancellationToken token);
        Task TranscodeMusic(IHttpContext context, string filePath, double seconds, CancellationToken token);
        string GetOutputTranscodeMimeType(string filepath);
    }
}
