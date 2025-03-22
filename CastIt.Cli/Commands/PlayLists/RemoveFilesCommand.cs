using CastIt.Cli.Interfaces.Api;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CastIt.Cli.Commands.PlayLists
{
    [Command(
        Name = "remove-files",
        Description = "Removes either missing files, the files that starts with an specific provided path or specific files from the playlist",
        OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    public class RemoveFilesCommand : BaseCommand
    {
        [Argument(0, Description = "The playlist id", ShowInHelpText = true)]
        public long PlayListId { get; set; }

        [Option(CommandOptionType.MultipleValue, Description = "Pass this one to delete specific files", LongName = "fileId", ShortName = "fileId")]
        public List<long> FileIds { get; set; } = new List<long>();

        [Option(CommandOptionType.SingleValue, Description = "Pass this one to remove only the files that starts with this path", LongName = "starts-with", ShortName = "starts-with")]
        public string StartsWithPath { get; set; }

        [Option(CommandOptionType.NoValue, Description = "Pass this one to remove only the missing files", LongName = "missing-only", ShortName = "missing-only")]
        public bool OnlyMissing { get; set; }

        public RemoveFilesCommand(IConsole appConsole, ICastItApiService castItApi)
            : base(appConsole, castItApi)
        {
        }

        protected override async Task<int> Execute(CommandLineApplication app)
        {
            if (FileIds.Any())
            {
                var response = await CastItApi.RemoveFiles(PlayListId, FileIds);
                CheckServerResponse(response);
                AppConsole.WriteLine($"Successfully removed fileIds = {string.Join(",", FileIds)}");
            }

            if (!string.IsNullOrWhiteSpace(StartsWithPath))
            {
                var response = await CastItApi.RemoveFilesThatStartsWith(PlayListId, StartsWithPath);
                CheckServerResponse(response);
                AppConsole.WriteLine($"Successfully removed files that starts with = {StartsWithPath}");
            }

            if (OnlyMissing)
            {
                var response = await CastItApi.RemoveAllMissingFiles(PlayListId);
                CheckServerResponse(response);
                AppConsole.WriteLine($"Successfully removed missing files from playlistId = {PlayListId}");
            }
            return SuccessCode;
        }
    }
}
