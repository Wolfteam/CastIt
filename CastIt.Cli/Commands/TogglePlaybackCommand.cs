﻿using CastIt.Application.Server;
using CastIt.Cli.Common.Utils;
using CastIt.Cli.Interfaces.Api;
using McMaster.Extensions.CommandLineUtils;
using Refit;
using System;
using System.Threading.Tasks;

namespace CastIt.Cli.Commands
{
    [Command(Name = "toggle-playback", Description = "Toggles the playback of the current played file")]
    public class TogglePlaybackCommand : BaseCommand
    {
        private readonly IConsole _console;

        public TogglePlaybackCommand(IConsole console)
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

                _console.WriteLine("Toggling playback of current played file...");
                var url = ServerUtils.StartServerIfNotStarted(_console);
                var castItApi = RestService.For<ICastItApi>(url);
                var response = await castItApi.TogglePlayback();
                if (!response.Succeed)
                {
                    _console.WriteLine(response.Message);
                    return -1;
                }

                _console.WriteLine("Playback was successfully toggled");
            }
            catch (Exception e)
            {
                _console.WriteLine(e.ToString());
            }
            return await base.OnExecute(app);
        }
    }
}
