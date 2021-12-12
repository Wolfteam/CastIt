using CastIt.Domain.Utils;
using CastIt.Shared.FilePaths;
using CastIt.Shared.Telemetry;
using Microsoft.Extensions.DependencyInjection;

namespace CastIt.Shared
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddFileService(
            this IServiceCollection services,
            string generatedFilesFolderPath = null)
        {
            if (string.IsNullOrWhiteSpace(generatedFilesFolderPath))
                generatedFilesFolderPath = AppFileUtils.GetBaseAppFolder();
            var fileService = new FileService(generatedFilesFolderPath);
            services.AddSingleton<IFileService>(fileService);
            return services.AddCommonFileService(fileService);
        }

        public static IServiceCollection AddCommonFileService(
            this IServiceCollection services,
            ICommonFileService instance = null)
        {
            if (instance != null)
            {
                services.AddSingleton(instance);
            }
            else
            {
                services.AddSingleton<ICommonFileService, CommonFileService>();
            }

            return services;
        }

        public static IServiceCollection AddTelemetry(this IServiceCollection services)
        {
            services.AddSingleton<ITelemetryService, TelemetryService>();
            return services;
        }
    }
}
