using CastIt.Cli.Interfaces.Api;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Threading.Tasks;

namespace CastIt.Cli.Commands.Files
{
    [Command(Name = "play", Description = "Plays a particular file to the connected device", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    public class PlayCommand : BaseCommand
    {
        public PlayCommand(IConsole appConsole, ICastItApiService castItApi)
            : base(appConsole, castItApi)
        {
        }

        [Option(CommandOptionType.SingleValue, Description = "The play list id", LongName = "playlist-id", ShortName = "playlist-id")]
        public long PlayListId { get; set; }

        [Option(CommandOptionType.SingleValue, Description = "The file id", LongName = "file-id", ShortName = "file-id")]
        public long FileId { get; set; }

        [Option(CommandOptionType.SingleValue, Description = "The filename", LongName = "filename", ShortName = "filename")]
        public string Filename { get; set; }

        [Option(CommandOptionType.SingleOrNoValue, Description = "Passing this param will force the playback of the file even if its already being played", LongName = "force", ShortName = "force")]
        public bool Force { get; set; }

        protected override async Task<int> Execute(CommandLineApplication app)
        {
            CheckIfWebServerIsRunning();
            if (string.IsNullOrWhiteSpace(Filename) && (PlayListId <= 0 || FileId <= 0))
            {
                AppConsole.WriteLine($"PlaylistId = {PlayListId}, FileId = {FileId} or Filename = {Filename} are not valid");
                return ErrorCode;
            }

            var response = !string.IsNullOrWhiteSpace(Filename)
                ? await CastItApi.Play(Filename, Force)
                : await CastItApi.Play(PlayListId, FileId);
            CheckServerResponse(response);
            AppConsole.WriteLine("File was successfully loaded");
            return SuccessCode;
        }
    }
}
