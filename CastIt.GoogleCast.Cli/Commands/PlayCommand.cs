using CastIt.Application.Interfaces;
using CastIt.Domain.Dtos.Requests;
using CastIt.GoogleCast.Cli.Common.Utils;
using CastIt.GoogleCast.Cli.Interfaces.Api;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using Refit;
using System;
using System.Threading.Tasks;

namespace CastIt.GoogleCast.Cli.Commands
{
    [Command(Name = "play", Description = "Plays a particular file to the connected device")]
    public class PlayCommand : BaseCommand
    {
        private readonly IConsole _console;
        private readonly ICommonFileService _fileService;

        public PlayCommand(
            IConsole console,
            ICommonFileService fileService)
        {
            _console = console;
            _fileService = fileService;
        }

        //TODO: HOW ARE WE SUPPOSED TO EDIT THE SETTINGS ?

        [Argument(0, Description = "The file's full path or url", ShowInHelpText = true)]
        public string Mrl { get; set; }

        [Option(CommandOptionType.SingleOrNoValue, Description = "The audio stream index, defaults to 0", LongName = "audio_index", ShortName = "ai")]
        public int AudioStreamIndex { get; set; }

        [Option(CommandOptionType.SingleOrNoValue, Description = "The video stream index, defaults to 0", LongName = "video_index", ShortName = "vi")]
        public int VideoStreamIndex { get; set; }

        [Option(CommandOptionType.SingleOrNoValue, Description = "The subtitles stream index, defaults to -1", LongName = "subs_index", ShortName = "si")]
        public int SubsStreamIndex { get; set; } = -1;

        [Option(CommandOptionType.SingleOrNoValue, Description = "The video quality, only required for youtube videos", LongName = "quality")]
        public int Quality { get; set; } = 360;

        [Option(CommandOptionType.SingleOrNoValue, Description = "The seconds where the file will be started", LongName = "seconds", ShortName = "sec")]
        public int Seconds { get; set; }

        protected override async Task<int> OnExecute(CommandLineApplication app)
        {
            try
            {
                var url = ServerUtils.StartServerIfNotStarted(_console);
                if (string.IsNullOrWhiteSpace(url))
                    return -1;

                bool isLocal = _fileService.IsLocalFile(Mrl);
                if (isLocal && !_fileService.Exists(Mrl))
                {
                    _console.WriteLine($"File = {Mrl} does not exist");
                    return -1;
                }

                _console.WriteLine($"Trying to play file = {Mrl}...");
                var request = isLocal
                    ? PlayCliFileRequestDto.FromLocal(Mrl, VideoStreamIndex, AudioStreamIndex, SubsStreamIndex)
                    : PlayCliFileRequestDto.FromYoutube(Mrl, Quality);

                if (Seconds > 0)
                    request.Seconds = Seconds;

                _console.WriteLine(
                    $"The following request will be send to the api = {JsonConvert.SerializeObject(request, Formatting.Indented)}, " +
                    $"the server url is = {url}");
                var castItApi = RestService.For<ICastItApi>(url);
                var response = await castItApi.Play(request);

                _console.WriteLine(response.Succeed
                    ? $"File = {Mrl} was successfully loaded"
                    : $"File couldn't be loaded. Error = {response.Message}");
            }
            catch (Exception e)
            {
                _console.WriteLine(e.ToString());
                return -1;
            }
            return await base.OnExecute(app);
        }
    }
}
