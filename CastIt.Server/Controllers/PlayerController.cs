using CastIt.Application.Interfaces;
using CastIt.Domain.Dtos;
using CastIt.Domain.Dtos.Requests;
using CastIt.Domain.Extensions;
using CastIt.Domain.Interfaces;
using CastIt.Domain.Models.FFmpeg.Transcode;
using CastIt.Infrastructure.Models;
using CastIt.Server.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CastIt.Server.Controllers
{
    public class PlayerController : BaseController<PlayerController>
    {
        private readonly IFileService _fileService;
        private readonly IFFmpegService _ffmpegService;
        private readonly IServerAppSettingsService _settingsService;

        public PlayerController(
            ILogger<PlayerController> logger,
            IFileService fileService,
            IFFmpegService fFmpegService,
            IServerCastService castService,
            IServerAppSettingsService settingsService)
            : base(logger, castService)
        {
            _fileService = fileService;
            _ffmpegService = fFmpegService;
            _settingsService = settingsService;
        }

        //TODO: ADD THE APPMESSAGE TYPE TO EACH RESPONSE ?
        [HttpGet("Devices")]
        public IActionResult GetAllDevices()
        {
            var response = new AppListResponseDto<IReceiver>
            {
                Succeed = true,
                Result = CastService.AvailableDevices
            };

            return Ok(response);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Connect(ConnectRequestDto dto)
        {
            Logger.LogInformation($"{nameof(Connect)}: Trying to connect to device by using host = {dto.Host} and port = {dto.Port}");
            await CastService.SetCastRenderer(dto.Host, dto.Port);

            Logger.LogInformation($"{nameof(Connect)}: Connection was successfully established");
            return Ok(new EmptyResponseDto(true));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Disconnect()
        {
            Logger.LogInformation($"{nameof(Disconnect)}: Trying to disconnect from device...");
            await CastService.SetCastRenderer(null);

            Logger.LogInformation($"{nameof(Disconnect)}: Disconnect successfully completed");
            return Ok(new EmptyResponseDto(true));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> TogglePlayback()
        {
            Logger.LogInformation($"{nameof(TogglePlayback)}: Toggling playback...");
            await CastService.TogglePlayback();

            Logger.LogInformation($"{nameof(TogglePlayback)}: Playback was successfully toggled");
            return Ok(new EmptyResponseDto(true));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Stop()
        {
            Logger.LogInformation($"{nameof(TogglePlayback)}: Stopping playback...");
            await CastService.StopPlayback();

            Logger.LogInformation($"{nameof(TogglePlayback)}: Stopping playback...");
            return Ok(new EmptyResponseDto(true));
        }

        [HttpPost("Volume")]
        public async Task<IActionResult> SetVolume(SetVolumeRequestDto dto)
        {
            if (dto.VolumeLevel < 0 || dto.VolumeLevel > 100)
            {
                return BadRequest(new EmptyResponseDto(false, $"VolumeLevel = {dto.VolumeLevel} is not valid"));
            }

            Logger.LogInformation($"{nameof(SetVolume)}: Setting volume level to = {dto.VolumeLevel} and muted to = {dto.IsMuted}...");

            await CastService.SetVolume(dto.VolumeLevel);

            await CastService.SetIsMuted(dto.IsMuted);

            Logger.LogInformation($"{nameof(SetVolume)}: Volume level was updated");
            return Ok(new EmptyResponseDto(true));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Next()
        {
            await CastService.GoTo(true);
            return Ok(new EmptyResponseDto(true));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Previous()
        {
            await CastService.GoTo(false);
            return Ok(new EmptyResponseDto(true));
        }

        [HttpPost("[action]/{seconds}")]
        public async Task<IActionResult> GoToSeconds(double seconds)
        {
            await CastService.GoToSeconds(seconds);
            return Ok(new EmptyResponseDto(true));
        }

        [HttpPost("[action]/{position}")]
        public async Task<IActionResult> GoToPosition(double position)
        {
            await CastService.GoToPosition(position);
            return Ok(new EmptyResponseDto(true));
        }

        [HttpPost("[action]/{seconds}")]
        public async Task<IActionResult> Seek(double seconds)
        {
            await CastService.AddSeconds(seconds);
            return Ok(new EmptyResponseDto(true));
        }

        [HttpGet("Settings")]
        public IActionResult GetSettings()
        {
            var response = new AppResponseDto<ServerAppSettings>(_settingsService.Settings);
            return Ok(response);
        }

        [HttpPatch("Settings")]
        public async Task<IActionResult> UpdateSettings(JsonPatchDocument<ServerAppSettings> patch)
        {
            if (patch == null)
                return BadRequest(new EmptyResponseDto(false, "You need to provide a valid object"));

            var updated = _settingsService.Settings.Copy();
            patch.ApplyTo(updated, ModelState);
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }

            await _settingsService.SaveSettings(updated);
            Logger.LogInformation($"{nameof(UpdateSettings)}: Settings were successfully updated");
            return Ok(new EmptyResponseDto(true));
        }

        [HttpGet("Status")]
        public IActionResult GetStatus()
        {
            var response = new AppResponseDto<ServerPlayerStatus>(CastService.GetPlayerStatus());
            return Ok(response);
        }

        #region Chromecast
        [HttpGet("[action]")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task Play([FromQuery] PlayAppFileRequestDto dto)
        {
            try
            {
                //TODO: MOVE THIS TO THE FFMPEG SERVICE
                var type = _fileService.GetFileType(dto.Mrl);
                //bool isVideoFile = _fileService.IsVideoFile(dto.Mrl);
                //bool isMusicFile = _fileService.IsMusicFile(dto.Mrl);
                //bool isHls = _fileService.IsHls(dto.Mrl);
                if (!type.IsLocalOrHls())
                {
                    Response.StatusCode = StatusCodes.Status400BadRequest;
                    Logger.LogWarning($"{nameof(Play)}: File = {dto.Mrl} is not a video nor music file.");
                    return;
                }

                //if (!isVideoFile && !isMusicFile && !isHls)
                //{
                //    Response.StatusCode = StatusCodes.Status400BadRequest;
                //    Logger.LogWarning($"{nameof(Play)}: File = {dto.Mrl} is not a video nor music file.");
                //    return;
                //}
                HttpContext.Response.ContentType = _ffmpegService.GetOutputTranscodeMimeType(dto.Mrl);
                DisableCaching();

                if (!type.IsMusic())
                {
                    var options = GetVideoFileOptions(dto);
                    Logger.LogInformation($"{nameof(Play)}: Handling request for video file with options = {JsonConvert.SerializeObject(options)}");
                    await _ffmpegService.TranscodeVideo(HttpContext.Response.Body, options).ConfigureAwait(false);
                }
                else
                {
                    var options = GetMusicFileOptions(dto);
                    Logger.LogInformation($"{nameof(Play)}: Handling request for music file with options = {JsonConvert.SerializeObject(options)}");
                    await using var memoryStream = await _ffmpegService.TranscodeMusic(options).ConfigureAwait(false);
                    //TODO: THIS LENGTH IS NOT WORKING PROPERLY
                    //TODO: NO CANCELLATION TOKEN HERE !!!
                    HttpContext.Response.ContentLength = memoryStream.Length;
                    await memoryStream.CopyToAsync(HttpContext.Response.Body).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Unknown error");
                Response.StatusCode = StatusCodes.Status500InternalServerError;
            }
        }

        [HttpGet("Images/{filename}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult GetImage(string filename)
        {
            var previewsPath = _fileService.GetPreviewsPath();
            var path = Path.Combine(previewsPath, filename);
            if (!System.IO.File.Exists(path))
            {
                return NotFound();
            }
            return PhysicalFile(path, "image/jpeg");
        }

        private static TranscodeVideoFile GetVideoFileOptions(PlayAppFileRequestDto dto)
        {
            return new TranscodeVideoFileBuilder()
                .WithDefaults(dto.HwAccelToUse, dto.VideoScale, dto.VideoWidthAndHeight)
                .WithStreams(dto.VideoStreamIndex, dto.AudioStreamIndex)
                .WithFile(dto.Mrl)
                .ForceTranscode(dto.VideoNeedsTranscode, dto.AudioNeedsTranscode)
                .GoTo(dto.Seconds)
                .Build();
        }

        private static TranscodeMusicFile GetMusicFileOptions(PlayAppFileRequestDto dto)
        {
            return new TranscodeMusicFileBuilder()
                .WithAudio(dto.AudioStreamIndex)
                .WithFile(dto.Mrl)
                .ForceTranscode(false, dto.AudioNeedsTranscode)
                .GoTo(dto.Seconds)
                .Build();
        }
        #endregion
    }
}