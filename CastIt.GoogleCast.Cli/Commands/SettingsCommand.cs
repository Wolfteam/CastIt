using CastIt.Application.Server;
using CastIt.Domain.Dtos.Requests;
using CastIt.Domain.Enums;
using CastIt.GoogleCast.Cli.Common.Utils;
using CastIt.GoogleCast.Cli.Interfaces.Api;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using Refit;
using System;
using System.Threading.Tasks;

namespace CastIt.GoogleCast.Cli.Commands
{
    [Command(Name = "settings", Description = "Updates or retrieves the settings of the web server. The updated settings will take effect starting from the next played file")]
    public class SettingsCommand : BaseCommand
    {
        private readonly IConsole _console;

        [Option(CommandOptionType.NoValue, Description = "If false, it will retrieve the current server settings, otherwise they will be updated. Defaults to false", LongName = "update")]
        public bool Update { get; set; }

        [Option(CommandOptionType.NoValue, Description = "Enables the hardware acceleration", LongName = "use_hwaccel", ShortName = "hw_accel")]
        public bool UseHardwareAcceleration { get; set; }

        [Option(CommandOptionType.NoValue, Description = "Forces the transcode of the video ", LongName = "transcode_video", ShortName = "tv")]
        public bool ForceVideoTranscode { get; set; }

        [Option(CommandOptionType.NoValue, Description = "Forces the transcode of the audio", LongName = "transcode_audio", ShortName = "ta")]
        public bool ForceAudioTranscode { get; set; }

        [Option(CommandOptionType.SingleOrNoValue, Description = "Sets the video scale of the video. Defaults to Original", LongName = "video_scale")]
        public VideoScaleType VideoScale { get; set; } = VideoScaleType.Original;

        public SettingsCommand(IConsole console)
        {
            _console = console;
        }

        protected override async Task<int> OnExecute(CommandLineApplication app)
        {
            try
            {
                if (!WebServerUtils.IsServerAlive())
                {
                    _console.WriteLine("Server is not running");
                    return -1;
                }

                var url = ServerUtils.StartServerIfNotStarted(_console);
                var castItApi = RestService.For<ICastItApi>(url);

                if (!Update)
                {
                    _console.WriteLine("Trying to retrieve the current server settings...");
                    var response = await castItApi.GetCurrentSettings();
                    if (!response.Succeed)
                    {
                        _console.WriteLine(response.Message);
                        return -1;
                    }

                    _console.WriteLine("Current server settings are:");
                    _console.WriteLine($"{JsonConvert.SerializeObject(response.Result, Formatting.Indented)}");
                }
                else
                {
                    var request = new UpdateCliAppSettingsRequestDto
                    {
                        ForceAudioTranscode = ForceAudioTranscode,
                        VideoScale = VideoScale,
                        EnableHardwareAcceleration = UseHardwareAcceleration,
                        ForceVideoTranscode = ForceVideoTranscode
                    };

                    _console.WriteLine($"Trying to update app settings with request = {JsonConvert.SerializeObject(request, Formatting.Indented)}...");

                    var response = await castItApi.UpdateAppSettings(request);
                    if (!response.Succeed)
                    {
                        _console.WriteLine(response.Message);
                        return -1;
                    }

                    _console.WriteLine("Settings were successfully updated. The new settings will take effect when you play a new file");
                }
            }
            catch (Exception e)
            {
                _console.WriteLine(e.ToString());
            }
            return await base.OnExecute(app);
        }
    }
}
