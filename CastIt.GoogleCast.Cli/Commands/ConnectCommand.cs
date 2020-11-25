using CastIt.Application.Interfaces;
using CastIt.Application.Server;
using CastIt.GoogleCast.Cli.Common.Utils;
using CastIt.GoogleCast.Cli.Interfaces.Api;
using CastIt.GoogleCast.Cli.Models;
using McMaster.Extensions.CommandLineUtils;
using Refit;
using System;
using System.Net;
using System.Threading.Tasks;

namespace CastIt.GoogleCast.Cli.Commands
{
    [Command(Name = "connect", Description = "Connects to a particular device")]
    public class ConnectCommand : BaseCommand
    {
        private readonly IConsole _console;
        private readonly ICommonFileService _fileService;
        private readonly AppSettings _appSettings;

        [Argument(0, Description = "The device´s ip address", ShowInHelpText = true)]
        public string IpAddress { get; set; }

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

        public ConnectCommand(IConsole console, ICommonFileService fileService, AppSettings appSettings)
        {
            _console = console;
            _appSettings = appSettings;
            _fileService = fileService;
        }

        protected override async Task<int> OnExecute(CommandLineApplication app)
        {
            _console.WriteLine($"Connecting to {IpAddress}...");
            try
            {
                if (string.IsNullOrWhiteSpace(IpAddress) || !IpAddress.Contains(":"))
                {
                    _console.WriteLine($"The provided ip address = {IpAddress} is not valid");
                    return await base.OnExecute(app);
                }

                var splitted = IpAddress.Split(':');
                bool hostIsValid = IPAddress.TryParse(splitted[0], out var deviceHost);
                bool portWasParsed = int.TryParse(splitted[1], out int devicePort);
                if (!portWasParsed || !hostIsValid)
                {
                    _console.WriteLine($"The provided ip address = {IpAddress} is not valid");
                    return await base.OnExecute(app);
                }

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
                if (string.IsNullOrWhiteSpace(url))
                    return -1;

                _console.WriteLine($"Trying to call the connect api of the web server = {url}");
                var castItApi = RestService.For<ICastItApi>(url);
                var response = await castItApi.Connect(deviceHost.ToString(), devicePort);
                _console.WriteLine(response.Succeed
                    ? "Connection completed"
                    : $"Connection to web server failed. Error = {response.Message}");
            }
            catch (Exception e)
            {
                _console.WriteLine(e.ToString());
            }

            return await base.OnExecute(app);
        }
    }
}
