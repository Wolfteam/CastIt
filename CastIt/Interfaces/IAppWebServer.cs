using CastIt.Common;
using CastIt.Interfaces.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading;

namespace CastIt.Interfaces
{
    public interface IAppWebServer : IDisposable
    {
        //OnCastRendererSetHandler OnCastRendererSet { get; set; }
        //OnCastableDeviceAddedHandler OnCastableDeviceAdded { get; set; }
        //OnCastableDeviceDeletedHandler OnCastableDeviceDeleted { get; set; }
        public OnFileLoading OnFileLoading { get; set; }
        public OnFileLoadedWs OnFileLoaded { get; set; }
        public OnFileLoadingError OnFileLoadingError { get; set; }
        OnPositionChangedHandler OnPositionChanged { get; set; }
        OnTimeChangedHandler OnTimeChanged { get; set; }
        OnEndReachedHandler OnEndReached { get; set; }
        //OnQualitiesChanged QualitiesChanged { get; set; }
        OnPaused OnPaused { get; set; }
        OnDisconnected OnDisconnected { get; set; }
        OnVolumeChanged OnVolumeChanged { get; set; }
        OnAppClosing OnAppClosing { get; set; }
        OnAppSettingsChanged OnAppSettingsChanged { get; set; }
        OnPlayListsChanged OnPlayListsChanged { get; set; }
        OnFilesChanged OnFilesChanged { get; set; }
        OnServerMsg OnServerMsg { get; set; }

        public string BaseUrl { get; }
        static IReadOnlyList<string> AllowedQueryParameters { get; }

        void Init(IMainViewModel mainViewModel, CancellationToken cancellationToken);

        string GetMediaUrl(string filePath, int videoStreamIndex, int audioStreamIndex, double seconds);

        string GetPreviewPath(string filepath);

        string GetSubTitlePath(string filepath);
    }
}
