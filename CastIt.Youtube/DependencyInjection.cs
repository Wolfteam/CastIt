using CastIt.Domain.Models.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace CastIt.Youtube
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddYoutubeParser(this IServiceCollection services)
        {
            services.AddSingleton<IYoutubeUrlDecoder, YoutubeUrlDecoder>();
            return services;
        }

        public static List<FileToLog> AddYoutubeParserLogs(this List<FileToLog> logs)
        {
            logs.AddRange(new List<FileToLog>
            {
                new FileToLog(typeof(YoutubeUrlDecoder), "decoder_youtube"),
            });
            return logs;
        }
    }
}
