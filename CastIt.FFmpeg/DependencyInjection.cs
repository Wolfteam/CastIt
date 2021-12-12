using CastIt.Domain.Models.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace CastIt.FFmpeg
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddFFmpeg(this IServiceCollection services)
        {
            services.AddSingleton<IFFmpegService, FFmpegService>();
            return services;
        }

        public static List<FileToLog> AddFFmpegLogs(this List<FileToLog> logs)
        {
            logs.AddRange(new List<FileToLog>
            {
                new FileToLog(typeof(FFmpegService), "service_ffmpeg"),
            });
            return logs;
        }
    }
}
