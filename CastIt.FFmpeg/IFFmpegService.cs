using CastIt.Domain.Enums;
using CastIt.Domain.Models.FFmpeg.Info;
using CastIt.Domain.Models.FFmpeg.Transcode;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CastIt.FFmpeg;

public interface IFFmpegService
{
    CancellationTokenSource TokenSource { get; }

    Task Init(string ffmpegExePath, string ffprobeExePath);

    void RefreshFfmpegPath(string ffmpegExePath, string ffprobeExePath);

    string GetThumbnail(long id, string mrl);

    Task GenerateThumbnails(long id, string mrl, bool hwAccelIsEnabled);

    Task KillThumbnailProcess();

    Task KillTranscodeProcess();

    Task<Stream> TranscodeVideo(TranscodeVideoFile options);

    Task<Stream> TranscodeMusic(TranscodeMusicFile options);

    Task<FFProbeFileInfo> GetFileInfo(string filePath, CancellationToken token);

    Task GenerateSubTitles(
        string filePath,
        string subtitleFinalPath,
        double seconds,
        int index,
        double subsDelayInSeconds,
        CancellationToken token);

    bool VideoNeedsTranscode(
        int videoStreamIndex,
        bool forceVideoTranscode,
        VideoScaleType selectedScale,
        FFProbeFileInfo fileInfo);

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