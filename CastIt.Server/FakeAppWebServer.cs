using CastIt.Application.Interfaces;
using CastIt.Application.Server;
using CastIt.Domain.Dtos.Requests;
using CastIt.Domain.Enums;
using CastIt.Shared.Extensions;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;

namespace CastIt.Server
{
    public class FakeAppWebServer : IBaseWebServer
    {
        private readonly ILogger<FakeAppWebServer> _logger;
        private readonly IServer _server;
        private string _baseIpAddress;

        #region Events
        //public OnFileLoadingHandler OnFileLoading { get; set; }
        //public OnFileLoadedWsHandler OnFileLoaded { get; set; }
        //public OnFileLoadingErrorHandler OnFileLoadingError { get; set; }
        //public OnPositionChangedHandler OnPositionChanged { get; set; }
        //public OnTimeChangedHandler OnTimeChanged { get; set; }
        //public OnEndReachedHandler OnEndReached { get; set; }
        //public OnPausedHandler OnPaused { get; set; }
        //public OnDisconnectedHandler OnDisconnected { get; set; }
        //public OnVolumeChangedHandler OnVolumeChanged { get; set; }
        //public OnAppClosingHandler OnAppClosing { get; set; }
        //public OnAppSettingsChangedHandler OnAppSettingsChanged { get; set; }

        //public OnPlayListAddedHandler OnPlayListAdded { get; set; }
        //public OnPlayListChangedHandler OnPlayListChanged { get; set; }
        //public OnPlayListDeletedHandler OnPlayListDeleted { get; set; }

        //public OnFileAddedHandler OnFileAdded { get; set; }
        //public OnFileChangedHandler OnFileChanged { get; set; }
        //public OnFileDeletedHandler OnFileDeleted { get; set; }

        //public OnServerMsgHandler OnServerMsg { get; set; }
        #endregion

        public FakeAppWebServer(ILogger<FakeAppWebServer> logger, IServer server)
        {
            _logger = logger;
            _server = server;
        }

        //Keep in mind that this method should be called AFTER the server has completely started
        public void Init()
        {
            var serverAddressesFeature = _server.Features.Get<IServerAddressesFeature>();
            //On iis express the IServerAddressesFeature will return the right ip,
            //but on other environments we will get something like http://[::]:9696 that's why we call
            //WebServerUtils.GetWebServerIpAddress()
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

        protected string GetBaseUrl()
        {
            CheckIpAddress();
            return $"{_baseIpAddress}/player";
        }

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
                VideoWidthAndHeight = videoWidthAndHeight
            };

            return SetUrlParameters($"{baseUrl}{AppWebServerConstants.MediaPath}", request);
        }

        public virtual string GetPreviewPath(string filepath)
        {
            if (string.IsNullOrEmpty(filepath))
                return null;
            var baseUrl = GetBaseUrl();
            string filename = Path.GetFileName(filepath);
            return $"{baseUrl}{AppWebServerConstants.ImagesPath}/{Uri.EscapeDataString(filename)}";
        }

        public virtual string GetSubTitlePath(string filepath)
        {
            var baseUrl = GetBaseUrl();
            string filename = Path.GetFileName(filepath);
            return $"{baseUrl}{AppWebServerConstants.SubTitlesPath}/{Uri.EscapeDataString(filename)}";
        }

        private string SetUrlParameters(string baseUrl, object dto)
        {
            return QueryHelpers.AddQueryString(baseUrl, dto.ToKeyValue());
        }

        public void Dispose()
        {
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
