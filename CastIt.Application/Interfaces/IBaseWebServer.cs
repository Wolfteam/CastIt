using CastIt.Application.Common;
using CastIt.Application.Server;
using CastIt.Domain.Enums;
using System;

namespace CastIt.Application.Interfaces
{
    public interface IBaseWebServer : IDisposable
    {
        //public OnFileLoadingHandler OnFileLoading { get; set; }
        //public OnFileLoadedWsHandler OnFileLoaded { get; set; }
        //public OnFileLoadingErrorHandler OnFileLoadingError { get; set; }
        //OnPositionChangedHandler OnPositionChanged { get; set; }
        //OnTimeChangedHandler OnTimeChanged { get; set; }
        //OnEndReachedHandler OnEndReached { get; set; }
        //OnPausedHandler OnPaused { get; set; }
        //OnDisconnectedHandler OnDisconnected { get; set; }
        //OnVolumeChangedHandler OnVolumeChanged { get; set; }
        //OnAppClosingHandler OnAppClosing { get; set; }
        //OnAppSettingsChangedHandler OnAppSettingsChanged { get; set; }

        //OnPlayListAddedHandler OnPlayListAdded { get; set; }
        //OnPlayListChangedHandler OnPlayListChanged { get; set; }
        //OnPlayListDeletedHandler OnPlayListDeleted { get; set; }

        //OnFileAddedHandler OnFileAdded { get; set; }
        //OnFileChangedHandler OnFileChanged { get; set; }
        //OnFileDeletedHandler OnFileDeleted { get; set; }

        //OnServerMsgHandler OnServerMsg { get; set; }

        void Init();

        public string GetMediaUrl(
            string filePath,
            int videoStreamIndex,
            int audioStreamIndex,
            double seconds,
            bool videoNeedsTranscode,
            bool audioNeedsTranscode,
            HwAccelDeviceType hwAccelToUse,
            VideoScaleType videoScale,
            string videoWidthAndHeight = null);

        string GetPreviewPath(string filepath);

        string GetSubTitlePath(string filepath);
    }
}
