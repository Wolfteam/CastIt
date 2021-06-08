using CastIt.Application.Interfaces;
using CastIt.Domain.Dtos;
using CastIt.Domain.Dtos.Requests;
using CastIt.Domain.Extensions;
using CastIt.Domain.Interfaces;
using CastIt.Domain.Models.FFmpeg.Transcode;
using CastIt.Test.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CastIt.Test.Controllers
{
    public class PlayerController : BaseController<PlayerController>
    {
        private readonly IFileService _fileService;
        private readonly IFFmpegService _ffmpegService;

        public PlayerController(
            ILogger<PlayerController> logger,
            IFileService fileService,
            IFFmpegService fFmpegService,
            IServerCastService castService) : base(logger, castService)
        {
            _fileService = fileService;
            _ffmpegService = fFmpegService;
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
            try
            {
                Logger.LogInformation($"{nameof(Connect)}: Trying to connect to device by using host = {dto.Host} and port = {dto.Port}");
                await CastService.SetCastRenderer(dto.Host, dto.Port);

                Logger.LogInformation($"{nameof(Connect)}: Connection was successfully established");
                return Ok(new EmptyResponseDto(true));
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"{nameof(Connect)}: Unknown error while trying to connect to host = {dto.Host} and port = {dto.Port}");
                return Ok(new EmptyResponseDto(false, e.Message));
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Disconnect()
        {
            try
            {
                Logger.LogInformation($"{nameof(Disconnect)}: Trying to disconnect from device...");
                await CastService.SetCastRenderer(null);

                Logger.LogInformation($"{nameof(Disconnect)}: Disconnect successfully completed");
                return Ok(new EmptyResponseDto(true));
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"{nameof(Disconnect)}: Unknown error occurred");
                return Ok(new EmptyResponseDto(false, e.Message));
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> TogglePlayback()
        {
            try
            {
                Logger.LogInformation($"{nameof(TogglePlayback)}: Toggling playback...");
                await CastService.TogglePlayback();

                Logger.LogInformation($"{nameof(TogglePlayback)}: Playback was successfully toggled");
                return Ok(new EmptyResponseDto(true));
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"{nameof(TogglePlayback)}: Unknown error occurred");
                return Ok(new EmptyResponseDto(false, e.Message));
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Stop()
        {
            try
            {
                Logger.LogInformation($"{nameof(TogglePlayback)}: Stopping playback...");
                await CastService.StopPlayback();

                Logger.LogInformation($"{nameof(TogglePlayback)}: Stopping playback...");
                return Ok(new EmptyResponseDto(true));
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"{nameof(Stop)}: Unknown error occurred");
                return Ok(new EmptyResponseDto(false, e.Message));
            }
        }

        [HttpPost("Volume")]
        public async Task<IActionResult> SetVolume(SetVolumeRequestDto dto)
        {
            try
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
            catch (Exception e)
            {
                Logger.LogError(e, $"{nameof(SetVolume)}: Unknown error occurred");
                return Ok(new EmptyResponseDto(false, e.Message));
            }
        }

        //TODO: THIS ONE ?
        [HttpPut("Settings")]
        public IActionResult UpdateSettings(UpdateCliAppSettingsRequestDto dto)
        {
            try
            {
                //_appSettings.EnableHardwareAcceleration = request.EnableHardwareAcceleration;
                //_appSettings.ForceAudioTranscode = request.ForceAudioTranscode;
                //_appSettings.ForceVideoTranscode = request.ForceVideoTranscode;
                //_appSettings.VideoScale = request.VideoScale;

                //Logger.LogInformation($"{nameof(UpdateSettings)}: Saving settings...");
                //_appSettings.SaveSettings();

                Logger.LogInformation($"{nameof(UpdateSettings)}: Settings were successfully updated");
                return Ok(new EmptyResponseDto(true));
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"{nameof(UpdateSettings)}: Unknown error occurred");
                return Ok(new EmptyResponseDto(false, e.Message));
            }
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