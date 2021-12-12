using CastIt.Domain.Models.Logging;
using CastIt.Youtube;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace CastIt.GoogleCast.Youtube
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddGoogleCastYoutube(this IServiceCollection services)
        {
            return services.AddYoutubeParser().AddYoutubeMediaRequestGenerator();
        }

        private static IServiceCollection AddYoutubeMediaRequestGenerator(this IServiceCollection services)
        {
            services.AddSingleton<YoutubeMediaRequestGenerator>();
            return services;
        }

        public static List<FileToLog> AddGoogleCastYoutubeLogs(this List<FileToLog> logs)
        {
            logs.AddRange(new List<FileToLog>
            {
                new FileToLog(typeof(YoutubeMediaRequestGenerator), "request_generator_youtube"),
            });
            return logs;
        }
    }
}
