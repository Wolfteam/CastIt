using CastIt.Common;
using CastIt.Models;
using CastIt.ViewModels.Items;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CastIt.Interfaces
{
    public interface ICastService
    {
        List<CastableDevice> AvailableDevices { get; }
        OnCastRendererSetHandler OnCastRendererSet { get; set; }
        OnCastableDeviceAddedHandler OnCastableDeviceAdded { get; set; }
        OnCastableDeviceDeletedHandler OnCastableDeviceDeleted { get; set; }
        OnEndReachedHandler OnEndReached { get; set; }
        OnPositionChangedHandler OnPositionChanged { get; set; }
        OnTimeChangedHandler OnTimeChanged { get; set; }

        Task AddSeconds(long seconds);
        Task GoToSeconds(long seconds);
        void CleanThemAll();
        bool DiscoverChromecasts();
        Task GoToPosition(float position);
        void Init();
        Task StartPlay(string mrl);
        string GetFirstThumbnail();
        string GetFirstThumbnail(string filePath);
        string GetThumbnail(int second);
        string GetThumbnail(string filePath, int second);
        Task StopPlayback();
        void TogglePlayback();
        Task<long> GetDuration(string mrl, CancellationToken cancellationToken = default);
        bool IsLocalFile(string mrl);
        bool IsUrlFile(string mrl);
        string GetFileName(string mrl);
        string GetExtension(string mrl);
        string GetFileSizeString(string mrl);
        void GenerateThumbmnails();
        void GenerateThumbmnails(string filePath);
        bool IsVideoFile(string mrl);
        bool IsMusicFile(string mrl);
        void SetCastRenderer(string name, string type);
    }
}