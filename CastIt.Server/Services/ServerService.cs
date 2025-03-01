using CastIt.Domain;
using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Entities;
using CastIt.Domain.Enums;
using CastIt.GoogleCast.Shared.Device;
using CastIt.Server.Interfaces;
using CastIt.Server.Shared;
using CastIt.Shared.FilePaths;
using CastIt.Shared.Models;
using CastIt.Shared.Server;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace CastIt.Server.Services
{
    public class ServerService : BaseServerService, IServerService
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
        public async Task Init()
        {
            int attempts = 5;
            while (attempts > 0)
            {
                attempts--;
                try
                {
                    var serverAddressesFeature = _server.Features.Get<IServerAddressesFeature>();
                    //On iis express the IServerAddressesFeature will return the right ip,
                    //but on other environments we will get something like http://[::]:9696 that's why we call
                    //WebServerUtils.GetWebServerIpAddress()
                    //Keep in mind that to test this thing on IIS you need to add to the configuration file (.vs\CastIt\config)
                    //<binding protocol="http" bindingInformation="*:62003:your.local.ipaddress" />
                    _logger.LogInformation($"The following addresses = {string.Join(",", serverAddressesFeature.Addresses)} will be checked...");
                    _baseIpAddress = GetLocalIpAddress(serverAddressesFeature.Addresses);
                    _logger.LogInformation($"The used ip address is going to be = {_baseIpAddress}");
                    break;
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, $"Could obtain local ip address, remaining attempts = {attempts}");
                    if (attempts <= 0)
                    {
                        _logger.LogError(e, "We were not able to get the local ip address");
                    }
                    //On a reboot / start, the pc may take a little bit to connect,
                    //that's why we give some time and retry
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }

            CheckIpAddress();
        }

        protected override string GetChromeCastBaseUrl()
        {
            CheckIpAddress();
            return $"{_baseIpAddress}/chromecast";
        }

        protected override string GetPlayerBaseUrl()
        {
            CheckIpAddress();
            return $"{_baseIpAddress}/player";
        }

        public override string GetOutputMimeType(string mrl)
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

        private void CheckIpAddress()
        {
            if (string.IsNullOrWhiteSpace(_baseIpAddress))
            {
                throw new NullReferenceException("The base ip address cannot be null");
            }
        }

        private string GetLocalIpAddress(ICollection<string> addresses)
        {
            if (addresses.Count == 0)
            {
                return null;
            }

            if (AppWebServerConstants.InDocker)
            {
                _logger.LogInformation("Running in container, retrieving url from environment variable...");
                string url = Environment.GetEnvironmentVariable("CASTIT_SERVER_URL");
                if (!string.IsNullOrWhiteSpace(url) && Uri.TryCreate(url, UriKind.Absolute, out _))
                {
                    return CleanUrl(url);
                }

                throw new InvalidOperationException(
                    "When running in a container, the server environment variable must be set " +
                    "and must be in the form of {Scheme}://{Host}:{Port}");
            }

            _logger.LogInformation("Running locally, retrieving url from {Addresses}...", addresses);
            string ipAddress = addresses.FirstOrDefault(s =>
                !s.Contains("*", StringComparison.OrdinalIgnoreCase) &&
                !s.Contains("localhost", StringComparison.OrdinalIgnoreCase) &&
                !s.Contains("[::]")) ?? WebServerUtils.GetWebServerIpAddress();

            if (string.IsNullOrWhiteSpace(ipAddress) && addresses.Count != 0)
            {
                int startPort = int.Parse(addresses.First().Split(':').Last());
                ipAddress = WebServerUtils.GetWebServerIpAddress(startPort);
            }

            return CleanUrl(ipAddress);

            static string CleanUrl(string url)
            {
                if (!string.IsNullOrWhiteSpace(url) && url.EndsWith('/'))
                {
                    url = url[..^1];
                }

                return url;
            }
        }
    }
}
