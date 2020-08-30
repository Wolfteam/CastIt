using CastIt.Common.Enums;
using CastIt.Models.FFMpeg;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CastIt.Interfaces
{
    public interface IFFMpegService
    {
        string GetThumbnail(string mrl, int second);
        Task GenerateThumbnails(string mrl);
        void KillThumbnailProcess();
        void KillTranscodeProcess();
        Task TranscodeVideo(Stream outputStream,
            string filePath,
            int videoStreamIndex,
            int audioStreamIndex,
            double seconds,
            bool videoNeedsTranscode,
            bool audioNeedsTranscode,
            HwAccelDeviceType hwAccelType,
            CancellationToken token,
            string videoWidthAndHeight = null);
        Task<MemoryStream> TranscodeMusic(
            string filePath,
            int audioStreamIndex,
            double seconds,
            CancellationToken token);
        string GetOutputTranscodeMimeType(string filepath);
        Task<FFProbeFileInfo> GetFileInfo(string filePath, CancellationToken token);
        Task GenerateSubTitles(string filePath, string subtitleFinalPath, double seconds, int index, CancellationToken token);

        bool VideoNeedsTranscode(int videoStreamIndex, FFProbeFileInfo fileInfo);

        bool AudioNeedsTranscode(int audioStreamIndex, FFProbeFileInfo fileInfo, bool checkIfAudioIsNull = false);

        HwAccelDeviceType GetHwAccelToUse(
            int videoStreamIndex,
            FFProbeFileInfo fileInfo,
            bool shouldUseHwAccel = true,
            bool isHls = false);
    }
}
