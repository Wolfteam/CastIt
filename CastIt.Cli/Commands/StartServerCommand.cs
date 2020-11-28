using CastIt.Application.Interfaces;
using CastIt.Application.Server;
using CastIt.Cli.Common.Utils;
using CastIt.Cli.Models;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Threading.Tasks;

namespace CastIt.Cli.Commands
{
    [Command(Name = "start-server", Description = "Starts the web server. This command should be called first before using any other command")]
    public class StartServerCommand : BaseCommand
    {
        private readonly IConsole _console;
        private readonly ICommonFileService _fileService;
        private readonly AppSettings _appSettings;

        [Option(
            CommandOptionType.SingleOrNoValue,
            Description = "The ffmpeg executable path that will be used by the server. If not provided, the default one in the app settings will be used",
            LongName = "ffmpeg_path",
            ShortName = "ffmpeg")]
        public string FFmpegPath { get; set; }

        [Option(
            CommandOptionType.SingleOrNoValue,
            Description = "The ffprobe executable path that will be used by the server. If not provided, the default one in the app settings will be used",
            LongName = "ffprobe_path",
            ShortName = "ffprobe")]
        public string FFprobePath { get; set; }

        public StartServerCommand(
            IConsole console,
            ICommonFileService fileService,
            AppSettings appSettings)
        {
            _console = console;
            _fileService = fileService;
            _appSettings = appSettings;
        }

        protected override async Task<int> OnExecute(CommandLineApplication app)
        {
            try
            {
                _console.WriteLine("Killing any existing server process...");
                WebServerUtils.KillServerProcess();

                if (string.IsNullOrWhiteSpace(FFmpegPath))
                {
                    _console.WriteLine($"Using the default path for ffmpeg = {_appSettings.FFmpegPath}...");
                    FFmpegPath = _appSettings.FFmpegPath;
                }

                if (string.IsNullOrWhiteSpace(FFprobePath))
                {
                    _console.WriteLine($"Using the default path for ffprobe = {_appSettings.FFprobePath}...");
                    FFprobePath = _appSettings.FFprobePath;
                }

                if (!_fileService.IsLocalFile(FFmpegPath))
                {
                    _console.WriteLine($"FFmpegPath = {FFmpegPath} is not valid ");
                    return -1;
                }

                if (!_fileService.IsLocalFile(FFprobePath))
                {
                    _console.WriteLine($"FFprobePath = {FFprobePath} is not valid ");
                    return -1;
                }

                var url = ServerUtils.StartServerIfNotStarted(_console, FFmpegPath, FFprobePath);
                _console.WriteLine(string.IsNullOrWhiteSpace(url)
                    ? "Server couldn't be started"
                    : "Server was started");
            }
            catch (Exception e)
            {
                _console.WriteLine(e.ToString());
            }

            return await base.OnExecute(app);
        }

    }
}
