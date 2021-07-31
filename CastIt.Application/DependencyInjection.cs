using CastIt.Application.Common.Utils;
using CastIt.Application.FFMpeg;
using CastIt.Application.FilePaths;
using CastIt.Application.Interfaces;
using CastIt.Application.Telemetry;
using CastIt.Application.Youtube;
using CastIt.Domain.Models.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace CastIt.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services)
            => services
                .AddFileService()
                .AddCommonAppServices()
                .AddFFmpegService();

        public static IServiceCollection AddApplication(
            this IServiceCollection services,
            string generatedFilesFolderPath)
            => services
                .AddFileService(generatedFilesFolderPath)
                .AddCommonAppServices()
                .AddFFmpegService();

        public static IServiceCollection AddFileService(
            this IServiceCollection services,
            string generatedFilesFolderPath = null)
        {
            if (string.IsNullOrWhiteSpace(generatedFilesFolderPath))
                generatedFilesFolderPath = AppFileUtils.GetBaseAppFolder();
            var fileService = new FileService(generatedFilesFolderPath);
            services.AddSingleton<ICommonFileService>(fileService);
            services.AddSingleton<IFileService>(fileService);
            return services;
        }

        public static IServiceCollection AddApplicationForCli(this IServiceCollection services)
        {
            services.AddSingleton<ICommonFileService, CommonFileService>();
            return services.AddCommonAppServices();
        }

        public static IServiceCollection AddFFmpegService(this IServiceCollection services)
        {
            services.AddSingleton<IFFmpegService, FFmpegService>();
            return services;
        }

        private static IServiceCollection AddCommonAppServices(this IServiceCollection services)
        {
            services.AddSingleton<ITelemetryService, TelemetryService>();
            services.AddSingleton<IYoutubeUrlDecoder, YoutubeUrlDecoder>();
            services.AddSingleton<IFileWatcherService, FileWatcherService>();
            return services;
        }
    }

    public static class LoggingExtensions
    {
        public static List<FileToLog> AddApplicationLogs(this List<FileToLog> logs)
        {
            logs.AddRange(new List<FileToLog>
            {
                new FileToLog(typeof(FFmpegService), "service_ffmpeg"),
                new FileToLog(typeof(FileWatcherService), "service_file_watcher"),
                new FileToLog(typeof(YoutubeUrlDecoder), "decoder_youtube"),
            });
            return logs;
        }
    }
}
