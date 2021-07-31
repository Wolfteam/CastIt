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
                   //Controllers
                   new FileToLog(typeof(PlayerController), "controller_castit"),
                   new FileToLog(typeof(PlayListsController), "controller_playlists"),
                   new FileToLog(typeof(ServerController), "controller_server"),

                   //Services
                   new FileToLog(typeof(AppDataService), "service_appdata"),
                   new FileToLog(typeof(CastItHostedService), "hosted_service_castit"),
                   new FileToLog(typeof(ServerAppSettingsService), "service_appsettings"),
                   new FileToLog(typeof(ServerCastService), "service_cast"),
                   new FileToLog(typeof(ServerService), "service_server"),

                   //Others
                   new FileToLog(typeof(Player), "castit_player"),
                   new FileToLog(typeof(CastItHub), "hub_castit")
                });
            return logs;
        }
    }
}
