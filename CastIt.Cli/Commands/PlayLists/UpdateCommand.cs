﻿using CastIt.Cli.Interfaces.Api;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Threading.Tasks;

namespace CastIt.Cli.Commands.PlayLists
{
    [Command(Name = "update", Description = "Updates a playlist", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    public class UpdateCommand : BaseCommand
    {
        [Argument(0, Description = "The playlist id", ShowInHelpText = true)]
        public long PlayListId { get; set; }

        [Option(CommandOptionType.SingleValue, Description = "The playlist name", LongName = "name", ShortName = "name")]
        public string Name { get; set; }

        [Option(CommandOptionType.SingleValue, Description = "The playlist position", LongName = "position", ShortName = "position")]
        public int? Position { get; set; }

        public UpdateCommand(IConsole appConsole, ICastItApiService castItApi)
            : base(appConsole, castItApi)
        {
        }

        protected override async Task<int> Execute(CommandLineApplication app)
        {
            CheckIfWebServerIsRunning();
            var response = await CastItApi.UpdatePlayList(PlayListId, Name, Position);
            CheckServerResponse(response);

            AppConsole.WriteLine("Playlist was successfully updated");
            return SuccessCode;
        }
    }
}