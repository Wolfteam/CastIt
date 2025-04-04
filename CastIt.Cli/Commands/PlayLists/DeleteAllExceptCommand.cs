using CastIt.Cli.Interfaces.Api;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Threading.Tasks;

namespace CastIt.Cli.Commands.PlayLists
{
    [Command(Name = "delete-all-except", Description = "Deletes all the play lists except the provided one", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    public class DeleteAllExceptCommand : BaseCommand
    {
        [Argument(0, Description = "The playlist id to be excluded", ShowInHelpText = true)]
        public long ExcludePlayListId { get; set; }

        public DeleteAllExceptCommand(IConsole appConsole, ICastItApiService castItApi)
            : base(appConsole, castItApi)
        {
        }

        protected override async Task<int> Execute(CommandLineApplication app)
        {
            var response = await CastItApi.DeleteAllPlayList(ExcludePlayListId);
            CheckServerResponse(response);

            AppConsole.WriteLine($"All play lists were  deleted except playListId = {ExcludePlayListId}");
            return SuccessCode;
        }
    }
}
