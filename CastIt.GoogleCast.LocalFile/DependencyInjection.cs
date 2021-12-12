using CastIt.Domain.Models.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace CastIt.GoogleCast.LocalFile
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddGoogleCastLocalFiles(this IServiceCollection services)
        {
            services.AddSingleton<LocalFileMediaRequestGenerator>();
            return services;
        }

        public static List<FileToLog> AddGoogleCastLocalFilesLogs(this List<FileToLog> logs)
        {
            logs.AddRange(new List<FileToLog>
            {
                new FileToLog(typeof(LocalFileMediaRequestGenerator), "request_generator_local_files"),
            });
            return logs;
        }
    }
}
