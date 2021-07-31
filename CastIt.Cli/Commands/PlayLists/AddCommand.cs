using CastIt.Cli.Interfaces.Api;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CastIt.Cli.Commands.PlayLists
{
    [Command(Name = "add", Description = "Adds folder / files to the specified playlist", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    public class AddCommand : BaseCommand
    {
        [Argument(0, Description = "The playlist id", ShowInHelpText = true)]
        public long Id { get; set; }

        [Option(CommandOptionType.MultipleValue, Description = "The folder path to add", LongName = "folder", ShortName = "folder")]
        public List<string> Folders { get; set; } = new List<string>();

        [Option(CommandOptionType.MultipleValue, Description = "The file path to add", LongName = "file", ShortName = "file")]
        public List<string> Files { get; set; } = new List<string>();

        [Option(CommandOptionType.SingleValue, Description = "The url add", LongName = "url", ShortName = "url")]
        public string Url { get; set; }

        [Option(CommandOptionType.NoValue, Description = "Pass this one if you only want the video otherwise it will parse the full playlist. This one applies only when you add a url", LongName = "only-video", ShortName = "only-video")]
        public bool OnlyVideo { get; set; }

        [Option(CommandOptionType.NoValue, Description = "Pass this one if you want to include sub folders. This one applies only when the Folders param is provided", LongName = "include-subfolders", ShortName = "include-subfolders")]
        public bool IncludeSubFolders { get; set; }

        public AddCommand(IConsole appConsole, ICastItApiService castItApi)
            : base(appConsole, castItApi)
        {
        }

        protected override async Task<int> Execute(CommandLineApplication app)
        {
            CheckIfWebServerIsRunning();

            if (Folders.Any())
            {
                AppConsole.WriteLine("Adding folders...");
                var response = await CastItApi.AddFolders(Id, IncludeSubFolders, Folders);
                CheckServerResponse(response);
            }

            if (Files.Any())
            {
                AppConsole.WriteLine("Adding files...");
                var response = await CastItApi.AddFiles(Id, Folders);
                CheckServerResponse(response);
            }

            if (!string.IsNullOrWhiteSpace(Url))
            {
                AppConsole.WriteLine("Adding url...");
                var response = await CastItApi.AddUrl(Id, Url, OnlyVideo);
                CheckServerResponse(response);
            }

            return SuccessCode;
        }
    }
}
