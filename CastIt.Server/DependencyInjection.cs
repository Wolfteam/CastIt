using CastIt.Application.Interfaces;
using CastIt.Domain.Models.Logging;
using CastIt.Server.Controllers;
using CastIt.Server.Interfaces;
using CastIt.Server.Modules;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace CastIt.Server
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAppWebServer(this IServiceCollection services)
        {
            services.AddSingleton<IAppWebServer, AppWebServer>();
            services.AddSingleton<IBaseWebServer>(provider => provider.GetRequiredService<IAppWebServer>());
            return services;
        }

        public static List<FileToLog> GetServerLogs()
        {
            return new List<FileToLog>
            {
                new FileToLog(typeof(AppWebServer), "web_server"),
                new FileToLog(typeof(CastItController), "web_server_castit_controller"),
                new FileToLog(typeof(MediaModule), "web_server_media_module"),
                new FileToLog(typeof(MediaWebSocketModule), "web_server_media_socket_module"),
            };
        }
    }
}
