using CastIt.Application.FFMpeg;
using CastIt.Application.FilePaths;
using CastIt.Application.Interfaces;
using CastIt.Application.Telemetry;
using CastIt.Application.Youtube;
using CastIt.Domain.Models.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.IO;

namespace CastIt.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, string ffmpegPath, string ffprobePath)
        {
            var fileService = new FileService(ffmpegPath, ffprobePath, Directory.GetCurrentDirectory());
            services.AddSingleton<ICommonFileService>(fileService);
            services.AddSingleton<IFileService>(fileService);
            return services.AddCommonServices().AddFFmpegService();
        }

        public static IServiceCollection AddApplicationForCli(this IServiceCollection services)
        {
            services.AddSingleton<ICommonFileService, CommonFileService>();
            return services.AddCommonServices();
        }

        public static IServiceCollection AddFFmpegService(this IServiceCollection services)
        {
            services.AddSingleton<IFFmpegService, FFmpegService>();
            return services;
        }

        public static List<FileToLog> GetApplicationLogs()
        {
            return new List<FileToLog>
            {
                new FileToLog(typeof(FFmpegService), "service_ffmpeg"),
                new FileToLog(typeof(FileWatcherService), "service_file_watcher"),
                new FileToLog(typeof(YoutubeUrlDecoder), "decoder_youtube"),
            };
        }

        private static IServiceCollection AddCommonServices(this IServiceCollection services)
        {
            services.AddSingleton<ITelemetryService, TelemetryService>();
            services.AddSingleton<IYoutubeUrlDecoder, YoutubeUrlDecoder>();
            services.AddSingleton<IFileWatcherService, FileWatcherService>();
            return services;
        }
    }
}
