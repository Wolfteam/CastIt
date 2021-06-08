using CastIt.Application.Interfaces;
using CastIt.Application.Server;
using CastIt.Cli.Common.Utils;
using CastIt.Cli.Interfaces.Api;
using CastIt.Cli.Models;
using McMaster.Extensions.CommandLineUtils;
using System.Threading.Tasks;

namespace CastIt.Cli.Commands.Player
{
    [Command(Name = "start-server", Description = "Starts the web server. This command should be called first before using any other command")]
    public class StartServerCommand : BaseCommand
    {
        private readonly IFileService _fileService;
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

        public StartServerCommand(IConsole appConsole, ICastItApiService castItApi, IFileService fileService, AppSettings appSettings)
            : base(appConsole, castItApi)
        {
            _fileService = fileService;
            _appSettings = appSettings;
        }

        protected override Task<int> Execute(CommandLineApplication app)
        {
            AppConsole.WriteLine("Killing any existing server process...");
            WebServerUtils.KillServerProcess();

            if (string.IsNullOrWhiteSpace(FFmpegPath))
            {
                AppConsole.WriteLine($"Using the default path for FFmpeg = {_appSettings.FFmpegPath}...");
                FFmpegPath = _appSettings.FFmpegPath;
            }

            if (string.IsNullOrWhiteSpace(FFprobePath))
            {
                AppConsole.WriteLine($"Using the default path for FFprobe = {_appSettings.FFprobePath}...");
                FFprobePath = _appSettings.FFprobePath;
            }

            if (!_fileService.IsLocalFile(FFmpegPath))
            {
                AppConsole.WriteLine($"FFmpegPath = {FFmpegPath} is not valid ");
                return Task.FromResult(ErrorCode);
            }

            if (!_fileService.IsLocalFile(FFprobePath))
            {
                AppConsole.WriteLine($"FFprobePath = {FFprobePath} is not valid ");
                return Task.FromResult(ErrorCode);
            }

            var url = ServerUtils.StartServerIfNotStarted(FFmpegPath, FFprobePath);
            AppConsole.WriteLine(string.IsNullOrWhiteSpace(url)
                ? "Server couldn't be started"
                : "Server was started");

            return Task.FromResult(SuccessCode);
        }
    }
}
