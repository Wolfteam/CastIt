using CastIt.Domain.Models.Logging;
using CastIt.GoogleCast;
using CastIt.Server.Controllers;
using CastIt.Server.Hubs;
using CastIt.Server.Services;
using System.Collections.Generic;

namespace CastIt.Server.Common.Extensions
{
    public static class LoggingExtensions
    {
        public static List<FileToLog> AddServerLogs(this List<FileToLog> logs)
        {
            logs.AddRange(
                new List<FileToLog>
                {
                   //TODO: LOGS
                   new FileToLog(typeof(FakeAppWebServer), "web_server"),
                   new FileToLog(typeof(CastItHostedService), "hosted_service_castit"),

                   //Controllers
                   new FileToLog(typeof(PlayerController), "controller_castit"),
                   new FileToLog(typeof(PlayListsController), "controller_playlists"),
                   new FileToLog(typeof(CastItHub), "hub_castit"),

                   //Services
                   new FileToLog(typeof(ServerCastService), "service_cast"),
                   new FileToLog(typeof(AppDataService), "service_appdata"),
                   new FileToLog(typeof(ServerAppSettingsService), "service_appsettings"),

                   //Others
                   new FileToLog(typeof(Player), "castit_player")
                });
            return logs;
        }
    }
}
