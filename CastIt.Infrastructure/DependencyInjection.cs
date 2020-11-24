using CastIt.Domain.Models.Logging;
using CastIt.GoogleCast;
using CastIt.GoogleCast.Interfaces;
using CastIt.Infrastructure.Interfaces;
using CastIt.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace CastIt.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<IAppSettingsService, AppSettingsService>();
            services.AddSingleton<IPlayer>(provider => new Player(provider.GetRequiredService<ILogger<Player>>()));
            services.AddSingleton<ICastService, CastService>();

            return services;
        }

        public static List<FileToLog> GetInfrastructureLogs()
        {
            return new List<FileToLog>
            {
                new FileToLog(typeof(CastService), "service_cast"),
                new FileToLog(typeof(AppSettingsService), "service_app_settings")
            };
        }
    }
}
