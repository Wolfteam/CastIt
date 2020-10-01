using CastIt.Common;
using CastIt.Common.Enums;
using CastIt.Interfaces.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading;

namespace CastIt.Interfaces
{
    public interface IAppWebServer : IDisposable
    {
        public OnFileLoadingHandler OnFileLoading { get; set; }
        public OnFileLoadedWsHandler OnFileLoaded { get; set; }
        public OnFileLoadingErrorHandler OnFileLoadingError { get; set; }
        OnPositionChangedHandler OnPositionChanged { get; set; }
        OnTimeChangedHandler OnTimeChanged { get; set; }
        OnEndReachedHandler OnEndReached { get; set; }
        OnPausedHandler OnPaused { get; set; }
        OnDisconnectedHandler OnDisconnected { get; set; }
        OnVolumeChangedHandler OnVolumeChanged { get; set; }
        OnAppClosingHandler OnAppClosing { get; set; }
        OnAppSettingsChangedHandler OnAppSettingsChanged { get; set; }

        OnPlayListAddedHandler OnPlayListAdded { get; set; }
        OnPlayListChangedHandler OnPlayListChanged { get; set; }
        OnPlayListDeletedHandler OnPlayListDeleted { get; set; }

        OnFileAddedHandlder OnFileAdded { get; set; }
        OnFileChangedHandler OnFileChanged { get; set; }
        OnFileDeletedHandler OnFileDeleted { get; set; }

        OnServerMsgHandler OnServerMsg { get; set; }

        public string BaseUrl { get; }
        static IReadOnlyList<string> AllowedQueryParameters { get; }

        void Init(IMainViewModel mainViewModel, CancellationToken cancellationToken);

        string GetMediaUrl(
            string filePath,
            int videoStreamIndex,
            int audioStreamIndex,
            double seconds,
            bool videoNeedsTranscode,
            bool audioNeedsTranscode,
            HwAccelDeviceType hwAccelToUse,
            string videoWidthAndHeight = null);

        string GetPreviewPath(string filepath);

        string GetSubTitlePath(string filepath);
    }
}
