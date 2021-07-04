using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Entities;
using CastIt.Domain.Enums;
using CastIt.Domain.Interfaces;
using CastIt.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CastIt.Server.Interfaces
{
    public interface IServerService
    {
        Action<IReceiver> OnCastRendererSet { get; set; }
        Action<IReceiver> OnCastableDeviceAdded { get; set; }
        Action<string> OnCastableDeviceDeleted { get; set; }
        Action<List<IReceiver>> OnCastDevicesChanged { get; set; }
        Action OnEndReached { get; set; }
        Action<double> OnPositionChanged { get; set; }
        Action<double> OnTimeChanged { get; set; }
        Action<int, List<int>> QualitiesChanged { get; set; }
        Action OnPaused { get; set; }
        Action OnDisconnected { get; set; }
        Action<double, bool> OnVolumeChanged { get; set; }
        Action<AppMessageType> OnServerMessage { get; set; }
        Action OnAppClosing { get; set; }
        Action<ServerAppSettings> OnSettingsChanged { get; set; }
        Action<GetAllPlayListResponseDto> OnPlayListAdded { get; set; }
        Action<GetAllPlayListResponseDto> OnPlayListChanged { get; set; }
        Action<List<GetAllPlayListResponseDto>> OnPlayListsChanged { get; set; }
        Action<long> OnPlayListDeleted { get; set; }
        Action<long, bool> OnPlayListBusy { get; set; }
        Action<FileItemResponseDto> OnFileAdded { get; set; }
        Action<FileItemResponseDto> OnFileChanged { get; set; }
        Action<List<FileItemResponseDto>> OnFilesChanged { get; set; }
        Action<long, long> OnFileDeleted { get; set; }
        Action<long, FileItem[]> OnFilesAdded { get; set; }
        Action<FileItemResponseDto> OnFileLoading { get; set; }
        Action<FileItemResponseDto> OnFileLoaded { get; set; }
        Action OnStoppedPlayback { get; set; }

        void Init();

        public string GetPlayUrl(string filePath,
            int videoStreamIndex,
            int audioStreamIndex,
            double seconds,
            bool videoNeedsTranscode,
            bool audioNeedsTranscode,
            HwAccelDeviceType hwAccelToUse,
            VideoScaleType videoScale,
            int selectedQuality,
            string videoWidthAndHeight = null);

        string GetChromeCastPreviewUrl(string filepath);

        string GetThumbnailPreviewUrl(long tentativeSecond);

        string GetSubTitleUrl();

        string GetOutputMimeType(string mrl);

        Task StopAsync();
    }
}
