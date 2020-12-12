using CastIt.Application.Common;
using CastIt.Application.Interfaces;
using CastIt.Domain.Enums;
using System;
using System.Collections.Generic;
using System.IO;

namespace CastIt.Application.Server
{
    public abstract class BaseWebServer : IBaseWebServer
    {
        #region Events
        public OnFileLoadingHandler OnFileLoading { get; set; }
        public OnFileLoadedWsHandler OnFileLoaded { get; set; }
        public OnFileLoadingErrorHandler OnFileLoadingError { get; set; }
        public OnPositionChangedHandler OnPositionChanged { get; set; }
        public OnTimeChangedHandler OnTimeChanged { get; set; }
        public OnEndReachedHandler OnEndReached { get; set; }
        public OnPausedHandler OnPaused { get; set; }
        public OnDisconnectedHandler OnDisconnected { get; set; }
        public OnVolumeChangedHandler OnVolumeChanged { get; set; }
        public OnAppClosingHandler OnAppClosing { get; set; }
        public OnAppSettingsChangedHandler OnAppSettingsChanged { get; set; }

        public OnPlayListAddedHandler OnPlayListAdded { get; set; }
        public OnPlayListChangedHandler OnPlayListChanged { get; set; }
        public OnPlayListDeletedHandler OnPlayListDeleted { get; set; }

        public OnFileAddedHandler OnFileAdded { get; set; }
        public OnFileChangedHandler OnFileChanged { get; set; }
        public OnFileDeletedHandler OnFileDeleted { get; set; }

        public OnServerMsgHandler OnServerMsg { get; set; }
        #endregion

        #region Properties
        public static IReadOnlyList<string> AllowedQueryParameters => AppWebServerConstants.AllowedQueryParameters;

        public string BaseUrl
            => GetBaseUrl();
        #endregion

        protected abstract string GetBaseUrl();

        public string GetMediaUrl(
            string filePath,
            int videoStreamIndex,
            int audioStreamIndex,
            double seconds,
            bool videoNeedsTranscode,
            bool audioNeedsTranscode,
            HwAccelDeviceType hwAccelToUse,
            VideoScaleType videoScale,
            string videoWidthAndHeight = null)
        {
            var baseUrl = GetBaseUrl();
            return $"{baseUrl}{AppWebServerConstants.MediaPath}?" +
                   $"{AppWebServerConstants.VideoStreamIndexParameter}={videoStreamIndex}" +
                   $"&{AppWebServerConstants.AudioStreamIndexParameter}={audioStreamIndex}" +
                   $"&{AppWebServerConstants.SecondsQueryParameter}={seconds}" +
                   $"&{AppWebServerConstants.FileQueryParameter}={Uri.EscapeDataString(filePath)}" +
                   $"&{AppWebServerConstants.VideoNeedsTranscode}={videoNeedsTranscode}" +
                   $"&{AppWebServerConstants.AudioNeedsTranscode}={audioNeedsTranscode}" +
                   $"&{AppWebServerConstants.HwAccelTypeToUse}={hwAccelToUse}" +
                   $"&{AppWebServerConstants.VideoWidthAndHeight}={videoWidthAndHeight}" +
                   $"&{AppWebServerConstants.VideoScaleParameter}={videoScale}";
        }

        public string GetPreviewPath(string filepath)
        {
            if (string.IsNullOrEmpty(filepath))
                return null;
            var baseUrl = GetBaseUrl();
            string filename = Path.GetFileName(filepath);
            return $"{baseUrl}{AppWebServerConstants.ImagesPath}/{Uri.EscapeDataString(filename)}";
        }

        public string GetSubTitlePath(string filepath)
        {
            var baseUrl = GetBaseUrl();
            string filename = Path.GetFileName(filepath);
            return $"{baseUrl}{AppWebServerConstants.SubTitlesPath}/{Uri.EscapeDataString(filename)}";
        }

        public abstract void Dispose();
    }
}
