using CastIt.Shared.Models.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Collections.Generic;
using System.IO;

namespace CastIt.Shared.Extensions
{
    public static class LoggingExtensions
    {
        public const string LoggingFormat = "{Timestamp:dd-MM-yyyy HH:mm:ss.fff} ({ThreadId}) [{Level}] {Message:lj}{NewLine}{Exception}";

        public static void SetupLogging(this List<FileToLog> logs, string logsPath = null)
        {
            string basePath = logsPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

            if (string.IsNullOrWhiteSpace(basePath))
                throw new ArgumentNullException(nameof(logsPath), "You need to provide a base path for the logs");

            var loggingConfig = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
                .Enrich.WithThreadId()
                .Enrich.FromLogContext()
                .WriteTo.Debug(outputTemplate: LoggingFormat)
                .WriteTo.Console(outputTemplate: LoggingFormat, theme: AnsiConsoleTheme.Literate);

            var tenMb = 10000000;
            foreach (var kvp in logs)
            {
                string filename = $"{kvp.LogFileName}_.txt";
                if (kvp.Filtered)
                {
                    loggingConfig.WriteTo
                        .Logger(l => l.Filter.ByIncludingOnly(Matching.FromSource(kvp.AssemblyFullName))
                            .WriteTo.File(
                                Path.Combine(basePath, filename),
                                rollingInterval: RollingInterval.Day,
                                fileSizeLimitBytes: tenMb,
                                rollOnFileSizeLimit: true,
                                outputTemplate: LoggingFormat));
                }
                else
                {
                    loggingConfig.WriteTo
                        .Logger(l => l
                            .WriteTo.File(
                                Path.Combine(basePath, filename),
                                rollingInterval: RollingInterval.Day,
                                fileSizeLimitBytes: tenMb,
                                rollOnFileSizeLimit: true,
                                outputTemplate: LoggingFormat));
                }
            }
            Log.Logger = loggingConfig.CreateLogger();
        }
    }
}
