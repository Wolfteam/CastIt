using CastIt.Common;
using CastIt.GoogleCast.Interfaces;
using CastIt.GoogleCast.Models.Media;
using System.Collections.Generic;
using System.Threading;
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

        Task<MediaStatus> AddSeconds(double seconds);
        Task<MediaStatus> GoToSeconds(double seconds);
        void CleanThemAll();
        Task<MediaStatus> GoToPosition(string filePath, double position, double totalSeconds);
        void Init();
        Task<MediaStatus> StartPlay(string mrl, double seconds = 0);
        string GetFirstThumbnail();
        string GetFirstThumbnail(string filePath);
        string GetThumbnail(int second);
        string GetThumbnail(string filePath, int second);
        Task StopPlayback();
        Task TogglePlayback();
        Task<double> GetDuration(string mrl, CancellationToken token);
        string GetFileName(string mrl);
        string GetExtension(string mrl);
        string GetFileSizeString(string mrl);
        void GenerateThumbmnails();
        void GenerateThumbmnails(string filePath);
        Task SetCastRenderer(string id);
    }
}