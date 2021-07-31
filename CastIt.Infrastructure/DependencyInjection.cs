using CastIt.Domain.Models.Logging;
using CastIt.GoogleCast;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace CastIt.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            //services.AddSingleton<IAppSettingsService, AppSettingsService>();
            //TODO: MOVE THIS TO THE GOOGLE CAST PROJECT
            services.AddGoogleCast();
            //services.AddSingleton<ICastService, CastService>();

            return services;
        }

        public static IServiceCollection AddServerInfrastructure(this IServiceCollection services)
        {
            //services.AddSingleton<IAppSettingsService, AppSettingsService>();
            services.AddGoogleCast();
            return services;
        }

        public static List<FileToLog> GetInfrastructureLogs()
        {
            return new List<FileToLog>
            {
                //new FileToLog(typeof(CastService), "service_cast"),
                //new FileToLog(typeof(AppSettingsService), "service_app_settings")
            };
        }
    }
}
