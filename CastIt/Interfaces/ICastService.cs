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
        void CleanThemAll();
        bool DiscoverChromecasts();
        void GoToPosition(float position);
        void Init();
        void StartPlay(string mrl, bool isLocal);
        void StopPlayback();
        void TogglePlayback();
        Task<long> GetDuration(string mrl, bool isLocal);
    }
}