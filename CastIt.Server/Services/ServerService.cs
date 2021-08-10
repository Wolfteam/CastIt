using CastIt.Application.Interfaces;
using CastIt.Application.Server;
using CastIt.Domain.Dtos.Requests;
using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Entities;
using CastIt.Domain.Enums;
using CastIt.Domain.Interfaces;
using CastIt.Infrastructure.Models;
using CastIt.Server.Interfaces;
using CastIt.Shared.Extensions;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CastIt.Server.Services
{
    public class ServerService : IServerService
    {
        private readonly ILogger<ServerService> _logger;
        private readonly IServer _server;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IFileService _fileService;
        private string _baseIpAddress;

        #region Player Delegates
        public Action<IReceiver> OnCastRendererSet { get; set; }
        public Action<IReceiver> OnCastableDeviceAdded { get; set; }
        public Action<string> OnCastableDeviceDeleted { get; set; }
        public Action<List<IReceiver>> OnCastDevicesChanged { get; set; }
        public Action<double> OnPositionChanged { get; set; }
        public Action<double> OnTimeChanged { get; set; }
        public Action OnEndReached { get; set; }
        public Action<int, List<int>> QualitiesChanged { get; set; }
        public Action OnPaused { get; set; }
        public Action OnDisconnected { get; set; }
        public Action<double, bool> OnVolumeChanged { get; set; }
        public Action<AppMessageType> OnServerMessage { get; set; }
        #endregion

        #region Server Delegates
        public Action OnAppClosing { get; set; }
        public Action<ServerAppSettings> OnSettingsChanged { get; set; }

        public Action<GetAllPlayListResponseDto> OnPlayListAdded { get; set; }
        public Action<GetAllPlayListResponseDto> OnPlayListChanged { get; set; }
        public Action<List<GetAllPlayListResponseDto>> OnPlayListsChanged { get; set; }
        public Action<long> OnPlayListDeleted { get; set; }
        public Action<long, bool> OnPlayListBusy { get; set; }

        public Action<FileItemResponseDto> OnFileAdded { get; set; }
        public Action<FileItemResponseDto> OnFileChanged { get; set; }
        public Action<List<FileItemResponseDto>> OnFilesChanged { get; set; }
        public Action<long, long> OnFileDeleted { get; set; }

        public Action<long, FileItem[]> OnFilesAdded { get; set; }
        public Action<FileItemResponseDto> OnFileLoading { get; set; }
        public Action<FileItemResponseDto> OnFileLoaded { get; set; }
        public Action OnStoppedPlayback { get; set; }
        #endregion

        public ServerService(
            ILogger<ServerService> logger,
            IServer server,
            IHostApplicationLifetime hostApplicationLifetime,
            IFileService fileService)
        {
            _logger = logger;
            _server = server;
            _hostApplicationLifetime = hostApplicationLifetime;
            _fileService = fileService;
        }

        //Keep in mind that this method should be called AFTER the server has completely started
        public void Init()
        {
            var serverAddressesFeature = _server.Features.Get<IServerAddressesFeature>();
            //On iis express the IServerAddressesFeature will return the right ip,
            //but on other environments we will get something like http://[::]:9696 that's why we call
            //WebServerUtils.GetWebServerIpAddress()
            //Keep in mind that to test this thing on IIS you need to add to the configuration file (.vs\CastIt\config)
            //<binding protocol="http" bindingInformation="*:62003:your.local.ipaddress" />
            var ipAddress = serverAddressesFeature.Addresses.FirstOrDefault(s =>
                !s.Contains("*", StringComparison.OrdinalIgnoreCase) &&
                !s.Contains("localhost", StringComparison.OrdinalIgnoreCase) &&
                !s.Contains("[::]")) ?? WebServerUtils.GetWebServerIpAddress();

            if (!string.IsNullOrWhiteSpace(ipAddress) && ipAddress.EndsWith("/"))
            {
                ipAddress = ipAddress[..^1];
            }

            _baseIpAddress = ipAddress;
            _logger.LogInformation($"The current ip addresses are = {string.Join(",", serverAddressesFeature.Addresses)} and the used will be = {_baseIpAddress}");
            CheckIpAddress();
        }

        protected string GetPlayerBaseUrl()
        {
            CheckIpAddress();
            return $"{_baseIpAddress}/player";
        }

        public string GetPlayUrl(
            string filePath,
            int videoStreamIndex,
            int audioStreamIndex,
            double seconds,
            bool videoNeedsTranscode,
            bool audioNeedsTranscode,
            HwAccelDeviceType hwAccelToUse,
            VideoScaleType videoScale,
            int selectedQuality,
            string videoWidthAndHeight = null)
        {
            var baseUrl = GetPlayerBaseUrl();
            var request = new PlayAppFileRequestDto
            {
                Mrl = filePath,
                VideoStreamIndex = videoStreamIndex,
                AudioStreamIndex = audioStreamIndex,
                Seconds = seconds,
                VideoNeedsTranscode = videoNeedsTranscode,
                AudioNeedsTranscode = audioNeedsTranscode,
                HwAccelToUse = hwAccelToUse,
                VideoScale = videoScale,
                SelectedQuality = selectedQuality,
                VideoWidthAndHeight = videoWidthAndHeight
            };

            return SetUrlParameters($"{baseUrl}/{AppWebServerConstants.ChromeCastPlayPath}", request);
        }

        public virtual string GetChromeCastPreviewUrl(string filepath)
        {
            if (string.IsNullOrEmpty(filepath))
                return null;
            string baseUrl = GetPlayerBaseUrl();
            string filename = Path.GetFileName(filepath);
            return $"{baseUrl}/{AppWebServerConstants.ChromeCastImagesPath}/{Uri.EscapeDataString(filename)}";
        }

        public virtual string GetThumbnailPreviewUrl(long tentativeSecond)
        {
            var baseUrl = GetPlayerBaseUrl();
            return $"{baseUrl}/{AppWebServerConstants.ThumbnailPreviewImagesPath}/{tentativeSecond}";
        }

        public virtual string GetSubTitleUrl()
        {
            var baseUrl = GetPlayerBaseUrl();
            return $"{baseUrl}/{AppWebServerConstants.ChromeCastSubTitlesPath}";
        }

        public string GetOutputMimeType(string mrl)
        {
            bool isVideoFile = _fileService.IsVideoFile(mrl);
            bool isMusicFile = _fileService.IsMusicFile(mrl);
            bool isHls = _fileService.IsHls(mrl);
            if (isVideoFile || isMusicFile || isHls)
            {
                //The transcode process generate either of these
                return isVideoFile || isHls ? "video/mp4" : "audio/aac";
            }

            return "video/webm";
        }

        private string SetUrlParameters(string baseUrl, object dto)
        {
            return QueryHelpers.AddQueryString(baseUrl, dto.ToKeyValue());
        }

        private void CheckIpAddress()
        {
            if (string.IsNullOrWhiteSpace(_baseIpAddress))
            {
                throw new NullReferenceException("The base ip address cannot be null");
            }
        }
    }
}
