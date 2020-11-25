using CastIt.Application.Interfaces;
using CastIt.Domain.Dtos;
using CastIt.Domain.Dtos.Requests;
using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Interfaces;
using CastIt.Domain.Models.FFmpeg.Info;
using CastIt.GoogleCast.Interfaces;
using CastIt.Infrastructure.Interfaces;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CastIt.Server.Controllers
{
    internal class CastItController : WebApiController
    {
        private readonly ILogger<CastItController> _logger;
        private readonly IFileService _fileService;
        private readonly ICastService _castService;
        private readonly IFFmpegService _ffmpegService;
        private readonly IPlayer _player;
        private readonly IAppSettingsService _appSettings;

        public CastItController(
            ILogger<CastItController> logger,
            IFileService fileService,
            ICastService castService,
            IFFmpegService ffmpegService,
            IPlayer player,
            IAppSettingsService appSettings)
        {
            _logger = logger;
            _fileService = fileService;
            _castService = castService;
            _ffmpegService = ffmpegService;
            _player = player;
            _appSettings = appSettings;
        }

        [Route(HttpVerbs.Get, "/image")]
        public async Task GetImgUrl([QueryField(true)] string filePath, [QueryField(true)] long tentativeSeconds)
        {
            var path = _fileService.GetClosestThumbnail(filePath, tentativeSeconds);

            if (!_fileService.Exists(path))
            {
                HttpContext.Response.StatusCode = 400;
                return;
            }

            await using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

            HttpContext.Response.ContentType = "image/jpeg";
            await using var stream = HttpContext.OpenResponseStream();
            await fs.CopyToAsync(stream).ConfigureAwait(false);
        }

        [Route(HttpVerbs.Get, "/devices")]
        public async Task<AppListResponseDto<IReceiver>> GetAllDevices([QueryField(true)] int timeout)
        {
            var response = new AppListResponseDto<IReceiver>();
            try
            {
                if (timeout < 1)
                {
                    timeout = 5;
                }
                _logger.LogInformation($"{nameof(GetAllDevices)}: Trying to get all devices in this network with a timeout of = {timeout} seconds...");
                response.Result = await _player.GetDevicesAsync(TimeSpan.FromSeconds(timeout));
                response.Succeed = true;

                _logger.LogInformation($"{nameof(GetAllDevices)}: Got = {response.Result.Count} device(s)");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(GetAllDevices)}: Unknown error");
                response.Message = e.Message;
            }

            return response;
        }

        [Route(HttpVerbs.Post, "/connect")]
        public async Task<EmptyResponseDto> Connect([QueryField(true)] string host, [QueryField(true)] int port)
        {
            try
            {
                _logger.LogInformation($"{nameof(Connect)}: Trying to connect to device by using host = {host} and port = {port}");
                await _castService.SetCastRenderer(host, port);

                _logger.LogInformation($"{nameof(Connect)}: Connection was successfully established");
                return new EmptyResponseDto(true);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(Connect)}: Unknown error while trying to connect to host = {host} and port = {port}");
                return new EmptyResponseDto(false, e.Message);
            }
        }

        [Route(HttpVerbs.Post, "/disconnect")]
        public EmptyResponseDto Disconnect()
        {
            try
            {
                _logger.LogInformation($"{nameof(Disconnect)}: Trying to disconnect from device...");
                _castService.SetCastRenderer(null);

                _logger.LogInformation($"{nameof(Disconnect)}: Disconnect successfully completed");
                return new EmptyResponseDto(true);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(Disconnect)}: Unknown error occurred");
                return new EmptyResponseDto(false, e.Message);
            }
        }

        [Route(HttpVerbs.Post, "/play")]
        public async Task<EmptyResponseDto> Play()
        {
            try
            {
                var request = await HttpContext.GetRequestDataAsync<PlayCliFileRequestDto>();
                if (request == null)
                {
                    _logger.LogWarning($"{nameof(Play)}: Nothing will be played, because no request was provided");
                    return new EmptyResponseDto(false, "Invalid request. You need to provide the play request object");
                }

                _logger.LogInformation($"{nameof(Play)}: Getting file info for = {request.Mrl}...");
                var fileInfo = await _ffmpegService.GetFileInfo(request.Mrl, default);
                if (fileInfo == null && _fileService.IsUrlFile(request.Mrl))
                {
                    fileInfo = new FFProbeFileInfo();
                }

                _logger.LogInformation(
                    $"{nameof(Play)}: Trying to play file = {JsonConvert.SerializeObject(request)}...");

                await _castService.StartPlay(request, fileInfo);

                _logger.LogInformation($"{nameof(Play)}: Mrl = {request.Mrl} was successfully loaded");
                return new EmptyResponseDto(true);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(Play)}: Unknown error occurred");
                return new EmptyResponseDto(false, e.Message);
            }
        }

        [Route(HttpVerbs.Post, "/toggle-playback")]
        public async Task<EmptyResponseDto> TogglePlayback()
        {
            try
            {
                _logger.LogInformation($"{nameof(TogglePlayback)}: Toggling playback...");
                await _castService.TogglePlayback();

                _logger.LogInformation($"{nameof(TogglePlayback)}: Playback was successfully toggled");
                return new EmptyResponseDto(true);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(TogglePlayback)}: Unknown error occurred");
                return new EmptyResponseDto(false, e.Message);
            }
        }

        [Route(HttpVerbs.Post, "/stop")]
        public async Task<EmptyResponseDto> Stop()
        {
            try
            {
                _logger.LogInformation($"{nameof(TogglePlayback)}: Stopping playback...");
                await _castService.StopPlayback();

                _logger.LogInformation($"{nameof(TogglePlayback)}: Stopping playback...");
                return new EmptyResponseDto(true);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(Stop)}: Unknown error occurred");
                return new EmptyResponseDto(false, e.Message);
            }
        }

        [Route(HttpVerbs.Post, "/goto-seconds")]
        public async Task<EmptyResponseDto> GoToSeconds()
        {
            try
            {
                var request = await HttpContext.GetRequestDataAsync<PlayCliFileRequestDto>();
                if (request == null)
                {
                    _logger.LogWarning($"{nameof(GoToSeconds)}: Nothing will be played, because no request was provided");
                    return new EmptyResponseDto(false, "Invalid request");
                }

                _logger.LogInformation($"{nameof(GoToSeconds)}: Getting file info for = {request.Mrl}...");
                var fileInfo = await _ffmpegService.GetFileInfo(request.Mrl, default);
                if (fileInfo == null && _fileService.IsUrlFile(request.Mrl))
                {
                    fileInfo = new FFProbeFileInfo();
                }

                _logger.LogInformation(
                    $"{nameof(GoToSeconds)}: Trying to play file = {JsonConvert.SerializeObject(request)} with seconds = {request.Seconds}...");

                await _castService.GoToSeconds(request, fileInfo);

                _logger.LogInformation($"{nameof(GoToSeconds)}: Mrl = {request.Mrl} was successfully loaded");

                return new EmptyResponseDto(true);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(GoToSeconds)}: Unknown error occurred");
                return new EmptyResponseDto(false, e.Message);
            }
        }

        [Route(HttpVerbs.Post, "/volume")]
        public async Task<EmptyResponseDto> SetVolume([QueryField(true)] double newLevel, [QueryField(true)] bool isMuted)
        {
            try
            {
                if (newLevel < 0 || newLevel > 100)
                {
                    return new EmptyResponseDto(false, $"VolumeLevel = {newLevel} is not valid");
                }

                _logger.LogInformation($"{nameof(GoToSeconds)}: Setting volume level to = {newLevel} and muted to = {isMuted}...");

                await _castService.SetVolume(newLevel);

                await _castService.SetIsMuted(isMuted);

                _logger.LogInformation($"{nameof(GoToSeconds)}: Volume level was updated");
                return new EmptyResponseDto(true);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(SetVolume)}: Unknown error occurred");
                return new EmptyResponseDto(false, e.Message);
            }
        }

        [Route(HttpVerbs.Get, "/settings")]
        public AppResponseDto<CliAppSettingsResponseDto> GetCurrentSettings()
        {
            return new AppResponseDto<CliAppSettingsResponseDto>
            {
                Succeed = true,
                Result = new CliAppSettingsResponseDto
                {
                    FFmpegBasePath = _fileService.GetFFmpegPath(),
                    FFprobeBasePath = _fileService.GetFFprobePath(),
                    ForceAudioTranscode = _appSettings.ForceAudioTranscode,
                    VideoScale = _appSettings.VideoScale,
                    EnableHardwareAcceleration = _appSettings.EnableHardwareAcceleration,
                    ForceVideoTranscode = _appSettings.EnableHardwareAcceleration
                }
            };
        }

        [Route(HttpVerbs.Post, "/settings")]
        public async Task<EmptyResponseDto> UpdateSettings()
        {
            try
            {
                var request = await HttpContext.GetRequestDataAsync<UpdateCliAppSettingsRequestDto>();
                if (request == null)
                {
                    _logger.LogWarning($"{nameof(UpdateSettings)}: Nothing will be updated since no settings were provided");
                    return new EmptyResponseDto(false, "Invalid request. You need to provide the settings request object");
                }

                _appSettings.EnableHardwareAcceleration = request.EnableHardwareAcceleration;
                _appSettings.ForceAudioTranscode = request.ForceAudioTranscode;
                _appSettings.ForceVideoTranscode = request.ForceVideoTranscode;
                _appSettings.VideoScale = request.VideoScale;

                _logger.LogInformation($"{nameof(UpdateSettings)}: Saving settings...");
                _appSettings.SaveSettings();

                _logger.LogInformation($"{nameof(UpdateSettings)}: Settings were successfully updated");
                return new EmptyResponseDto(true);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(UpdateSettings)}: Unknown error occurred");
                return new EmptyResponseDto(false, e.Message);
            }
        }
    }
}
