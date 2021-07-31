using CastIt.Domain.Enums;
using CastIt.Domain.Models.FFmpeg.Info;
using CastIt.Domain.Models.FFmpeg.Transcode;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CastIt.Application.Interfaces
{
    public interface IFFmpegService
    {
        CancellationTokenSource TokenSource { get; }

        Task Init(string ffmpegExePath, string ffprobeExePath);

        void RefreshFfmpegPath(string ffmpegExePath, string ffprobeExePath);

        string GetThumbnail(string mrl);

        Task GenerateThumbnails(string mrl, bool hwAccelIsEnabled);

        Task KillThumbnailProcess();

        Task KillTranscodeProcess();

        Task TranscodeVideo(Stream outputStream, TranscodeVideoFile options);

        Task<MemoryStream> TranscodeMusic(TranscodeMusicFile options);

        Task<FFProbeFileInfo> GetFileInfo(string filePath, CancellationToken token);

        Task GenerateSubTitles(
            string filePath,
            string subtitleFinalPath,
            double seconds,
            int index,
            double subsDelayInSeconds,
            CancellationToken token);

        bool VideoNeedsTranscode(int videoStreamIndex, bool forceVideoTranscode, VideoScaleType selectedScale, FFProbeFileInfo fileInfo);

        bool AudioNeedsTranscode(
            int audioStreamIndex,
            bool forceAudioTranscode,
            FFProbeFileInfo fileInfo,
            bool checkIfAudioIsNull = false);

        HwAccelDeviceType GetHwAccelToUse(
            int videoStreamIndex,
            FFProbeFileInfo fileInfo,
            bool shouldUseHwAccel = true,
            bool isHls = false);

        Task<byte[]> GetThumbnailTile(string mrl, long tentativeSecond, int fps);
    }
}
