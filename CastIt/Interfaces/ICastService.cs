using CastIt.Common;
using CastIt.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CastIt.Interfaces
{
    public interface ICastService
    {
        List<CastableDevice> AvailableDevices { get; }
        OnCastableDeviceAddedHandler OnCastableDeviceAdded { get; set; }
        OnEndReachedHandler OnEndReached { get; set; }
        OnPositionChangedHandler OnPositionChanged { get; set; }
        OnTimeChangedHandler OnTimeChanged { get; set; }

        void AddSeconds(long seconds);
        void GoToSeconds(long seconds);
        void CleanThemAll();
        bool DiscoverChromecasts();
        void GoToPosition(float position);
        void Init();
        Task StartPlay(string mrl);
        string GetFirstThumbnail();
        string GetFirstThumbnail(string filePath);
        string GetThumbnail(int second);
        string GetThumbnail(string filePath, int second);
        void StopPlayback();
        void TogglePlayback();
        Task<long> GetDuration(string mrl);
        bool IsLocalFile(string mrl);
        bool IsUrlFile(string mrl);
        string GetFileName(string mrl);
        string GetExtension(string mrl);
        string GetFileSizeString(string mrl);
        void GenerateThumbmnails();
        void GenerateThumbmnails(string filePath);
        bool IsVideoFile(string mrl);
        bool IsMusicFile(string mrl);
    }
}