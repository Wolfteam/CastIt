using CastIt.Common;
using CastIt.GoogleCast.Interfaces;
using CastIt.GoogleCast.Models.Media;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CastIt.Interfaces
{
    public interface ICastService
    {
        IList<IReceiver> AvailableDevices { get; }
        OnCastRendererSetHandler OnCastRendererSet { get; set; }
        OnCastableDeviceAddedHandler OnCastableDeviceAdded { get; set; }
        OnCastableDeviceDeletedHandler OnCastableDeviceDeleted { get; set; }
        OnEndReachedHandler OnEndReached { get; set; }
        OnPositionChangedHandler OnPositionChanged { get; set; }
        OnTimeChangedHandler OnTimeChanged { get; set; }
        OnQualitiesChanged QualitiesChanged { get; set; }
        OnPaused OnPaused { get; set; }
        OnDisconnected OnDisconnected { get; set; }

        void Init();
        void CleanThemAll();
        void StopRunningProcess();
        Task<MediaStatus> AddSeconds(
            int videoStreamIndex,
            int audioStreamIndex,
            int subtitleStreamIndex,
            int quality,
            double seconds);
        Task<MediaStatus> GoToSeconds(
            int videoStreamIndex,
            int audioStreamIndex,
            int subtitleStreamIndex,
            int quality,
            double seconds);
        Task<MediaStatus> GoToPosition(
            string filePath,
            int videoStreamIndex,
            int audioStreamIndex,
            int subtitleStreamIndex,
            int quality,
            double position,
            double totalSeconds);
        Task<MediaStatus> StartPlay(
            string mrl,
            int videoStreamIndex,
            int audioStreamIndex,
            int subtitleStreamIndex,
            int quality,
            double seconds = 0);
        string GetFirstThumbnail();
        string GetFirstThumbnail(string filePath);
        string GetThumbnail(int second);
        string GetThumbnail(string filePath, int second);
        Task StopPlayback();
        Task TogglePlayback();
        string GetFileName(string mrl);
        string GetExtension(string mrl);
        string GetFileSizeString(string mrl);
        void GenerateThumbmnails();
        void GenerateThumbmnails(string filePath);
        Task SetCastRenderer(string id);
    }
}