using CastIt.Domain.Dtos;
using CastIt.Domain.Dtos.Requests;
using CastIt.Domain.Dtos.Responses;
using CastIt.GoogleCast.Shared.Device;
using CastIt.Server.Interfaces;
using CastIt.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;

namespace CastIt.Server.Controllers
{
    public class PlayerController : BaseController<PlayerController>
    {
        private readonly IServerAppSettingsService _settingsService;

        public PlayerController(
            ILogger<PlayerController> logger,
            IServerCastService castService,
            IServerAppSettingsService settingsService)
            : base(logger, castService)
        {
            _settingsService = settingsService;
        }

        //TODO: ADD A SETTING THAT ALLOWS YOU TO CHANGE THE MAXIMUM DAYS TO WAIT BEFORE DELETING PREVIEWS

        /// <summary>
        /// Gets the player status (It may contain the current played file and playlist)
        /// </summary>
        /// <returns>The player status</returns>
        [HttpGet("Status")]
        [ProducesResponseType(typeof(AppResponseDto<ServerPlayerStatusResponseDto>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetStatus()
        {
            var response = new AppResponseDto<ServerPlayerStatusResponseDto>(CastService.GetPlayerStatus());
            return Ok(response);
        }

        /// <summary>
        /// Returns a list of all the available devices on the network
        /// </summary>
        /// <returns>A list of devices</returns>
        [HttpGet("Devices")]
        [ProducesResponseType(typeof(AppListResponseDto<IReceiver>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetAllDevices()
        {
            var response = new AppListResponseDto<IReceiver>
            {
                Succeed = true,
                Result = CastService.AvailableDevices
            };

            return Ok(response);
        }

        /// <summary>
        /// Refreshes the devices for the provided amount of <paramref name="seconds"/>
        /// </summary>
        /// <param name="seconds">The seconds to scan the network</param>
        /// <returns>An updated list of devices</returns>
        [HttpPost("Devices/Refresh/{seconds}")]
        [ProducesResponseType(typeof(EmptyResponseDto), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> RefreshDevices(double seconds)
        {
            await CastService.RefreshCastDevices(TimeSpan.FromSeconds(seconds));
            return Ok(new EmptyResponseDto(true));
        }

        /// <summary>
        /// Connects to a particular device
        /// </summary>
        /// <param name="dto">The request</param>
        /// <returns>Returns the result of the operation</returns>
        [HttpPost("[action]")]
        [ProducesResponseType(typeof(EmptyResponseDto), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> Connect(ConnectRequestDto dto)
        {
            Logger.LogInformation($"{nameof(Connect)}: Trying to connect to device by using host = {dto.Host} and port = {dto.Port}");
            await CastService.SetCastRenderer(dto.Host, dto.Port);

            Logger.LogInformation($"{nameof(Connect)}: Connection was successfully established");
            return Ok(new EmptyResponseDto(true));
        }

        /// <summary>
        /// Disconnects from the current connected device (if any)
        /// </summary>
        /// <returns>Returns the result of the operation</returns>
        [HttpPost("[action]")]
        [ProducesResponseType(typeof(EmptyResponseDto), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> Disconnect()
        {
            Logger.LogInformation($"{nameof(Disconnect)}: Trying to disconnect from device...");
            await CastService.SetCastRenderer(null);

            Logger.LogInformation($"{nameof(Disconnect)}: Disconnect successfully completed");
            return Ok(new EmptyResponseDto(true));
        }

        /// <summary>
        /// Toggles the playback. If it is paused, it resumes the playback,
        /// otherwise it will be paused
        /// </summary>
        /// <returns>Returns the result of the operation</returns>
        [HttpPost("[action]")]
        [ProducesResponseType(typeof(EmptyResponseDto), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> TogglePlayback()
        {
            Logger.LogInformation($"{nameof(TogglePlayback)}: Toggling playback...");
            await CastService.TogglePlayback();

            Logger.LogInformation($"{nameof(TogglePlayback)}: Playback was successfully toggled");
            return Ok(new EmptyResponseDto(true));
        }

        /// <summary>
        /// Stops the playback of the current played file (if any)
        /// </summary>
        /// <returns>Returns the result of the operation</returns>
        [HttpPost("[action]")]
        [ProducesResponseType(typeof(EmptyResponseDto), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> Stop()
        {
            Logger.LogInformation($"{nameof(TogglePlayback)}: Stopping playback...");
            await CastService.StopPlayback();

            Logger.LogInformation($"{nameof(TogglePlayback)}: Stopping playback...");
            return Ok(new EmptyResponseDto(true));
        }

        /// <summary>
        /// Sets the volume of the current connected device
        /// </summary>
        /// <param name="dto">The request</param>
        /// <returns>Returns the result of the operation</returns>
        [HttpPost("Volume")]
        [ProducesResponseType(typeof(EmptyResponseDto), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
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

        /// <summary>
        /// Tries to go to the next file in the current playlist
        /// </summary>
        /// <returns>Returns the result of the operation</returns>
        [HttpPost("[action]")]
        [ProducesResponseType(typeof(EmptyResponseDto), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> Next()
        {
            await CastService.GoTo(true);
            return Ok(new EmptyResponseDto(true));
        }

        /// <summary>
        /// Tries to go to the previous file in the current playlist
        /// </summary>
        /// <returns>Returns the result of the operation</returns>
        [HttpPost("[action]")]
        [ProducesResponseType(typeof(EmptyResponseDto), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> Previous()
        {
            await CastService.GoTo(false);
            return Ok(new EmptyResponseDto(true));
        }

        /// <summary>
        /// Goes to the specified <paramref name="seconds"/> in the current played file
        /// </summary>
        /// <param name="seconds">The seconds</param>
        /// <returns>Returns the result of the operation</returns>
        [HttpPost("[action]/{seconds}")]
        [ProducesResponseType(typeof(EmptyResponseDto), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> GoToSeconds(double seconds)
        {
            await CastService.GoToSeconds(seconds);
            return Ok(new EmptyResponseDto(true));
        }

        /// <summary>
        /// Goes to the specified <paramref name="position"/> in the current played file
        /// </summary>
        /// <param name="position">The position (0 - 100%)</param>
        /// <returns>Returns the result of the operation</returns>
        [HttpPost("[action]/{position}")]
        [ProducesResponseType(typeof(EmptyResponseDto), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> GoToPosition(double position)
        {
            await CastService.GoToPosition(position);
            return Ok(new EmptyResponseDto(true));
        }

        /// <summary>
        /// Seeks the current played file by the provided <paramref name="seconds"/>
        /// </summary>
        /// <param name="seconds">The seconds to add / subtract to the current played file</param>
        /// <returns>Returns the result of the operation</returns>
        [HttpPost("[action]/{seconds}")]
        [ProducesResponseType(typeof(EmptyResponseDto), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> Seek(double seconds)
        {
            await CastService.AddSeconds(seconds);
            return Ok(new EmptyResponseDto(true));
        }

        /// <summary>
        /// Returns the current server settings
        /// </summary>
        /// <returns>Returns the server settings</returns>
        [HttpGet("Settings")]
        [ProducesResponseType(typeof(AppResponseDto<ServerAppSettings>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetSettings()
        {
            var response = new AppResponseDto<ServerAppSettings>(_settingsService.Settings);
            return Ok(response);
        }

        /// <summary>
        /// Partially updates the server settings with the provided values in the patch
        /// </summary>
        /// <param name="patch">The values to update</param>
        /// <returns>Returns the result of the operation</returns>
        [HttpPatch("Settings")]
        [ProducesResponseType(typeof(AppResponseDto<ServerAppSettings>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
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
            await _settingsService.UpdateSettings(updated, true);
            Logger.LogInformation($"{nameof(UpdateSettings)}: Settings were successfully updated");
            return Ok(new EmptyResponseDto(true));
        }

        /// <summary>
        /// Retrieves a preview in the <paramref name="tentativeSecond"/>
        /// of the current played file
        /// </summary>
        /// <param name="tentativeSecond">The second to retrieve the preview</param>
        /// <returns>The preview thumbnail</returns>
        [HttpGet("Images/Previews/{tentativeSecond}")]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> GetPreviewImageForPlayedFile(long tentativeSecond)
        {
            Logger.LogTrace(
                $"{nameof(GetPreviewImageForPlayedFile)}: Checking if we can retrieve an image for " +
                $"file = {CastService.CurrentPlayedFile?.Filename} on second = {tentativeSecond}...");
            DisableCaching();
            var bytes = await CastService.GetClosestPreviewThumbnail(tentativeSecond);
            return File(new MemoryStream(bytes), MediaTypeNames.Image.Jpeg);
        }

        /// <summary>
        /// Plays the first file that matches the provided filename
        /// </summary>
        /// <param name="dto">The request dto</param>
        /// <returns>The result of the operation</returns>
        [HttpPost("[action]")]
        [ProducesResponseType(typeof(EmptyResponseDto), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> Play(PlayFileFromNameRequestDto dto)
        {
            await CastService.PlayFile(dto.Filename, dto.Force, false);
            return Ok(new EmptyResponseDto(true));
        }

        /// <summary>
        /// Sets the provided file options to the current played file
        /// </summary>
        /// <param name="dto">The request dto</param>
        /// <returns>The result of the operation</returns>
        [HttpPost("[action]")]
        [ProducesResponseType(typeof(EmptyResponseDto), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> SetCurrentPlayedFileOptions(SetMultiFileOptionsRequestDto dto)
        {
            await CastService.SetCurrentPlayedFileOptions(dto.AudioStreamIndex, dto.SubtitleStreamIndex, dto.Quality);
            return Ok(new EmptyResponseDto(true));
        }
    }
}