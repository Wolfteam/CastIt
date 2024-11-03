using System;
using System.IO;
using CastIt.Shared.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Formatting.Json;
using ILogger = Serilog.ILogger;

namespace CastIt.Shared.Extensions;

public static class LoggingExtensions
{
    public const string SimpleLoggingFormat = "({ThreadId}) [{Level}] {Message:lj}{NewLine}{Exception}";

    public const string FullLoggingFormat = "{Timestamp:dd-MM-yyyy HH:mm:ss.fff} " + SimpleLoggingFormat;

    public const int TenMb = 10_000_000;

    private static void TryCreateLogFolder(string path)
    {
        if (Directory.Exists(path))
        {
            return;
        }

        try
        {
            Directory.CreateDirectory(path);
        }
        catch
        {
            // ignored
        }
    }

    public static ILogger CreateBootstrapperLogger(ToLog? log = null, string? logsPath = null)
    {
        if (!string.IsNullOrWhiteSpace(logsPath))
        {
            TryCreateLogFolder(logsPath);
        }

        var config = new LoggerConfiguration()
            .Enrich.WithThreadId()
            .Enrich.FromLogContext()
            .WriteTo.Debug(outputTemplate: FullLoggingFormat)
            .WriteTo.Console(new JsonFormatter());

        if (!string.IsNullOrWhiteSpace(logsPath) && log != null)
        {
            config.WriteTo.File(
                Path.Combine(logsPath, log.LogFileName),
                rollingInterval: RollingInterval.Day,
                fileSizeLimitBytes: TenMb,
                rollOnFileSizeLimit: true,
                outputTemplate: FullLoggingFormat
            );
        }

        return config.CreateBootstrapLogger();
    }

    public static IHostBuilder ConfigureAppLogging(
        this IHostBuilder builder,
        string? logsPath = null,
        bool useJsonFormatOnFiles = false,
        bool useJsonFormatOnConsole = false,
        params ToLog[] logs)
    {
        bool logToFile = !string.IsNullOrWhiteSpace(logsPath);
        if (logToFile)
        {
            TryCreateLogFolder(logsPath!);
        }

        return builder.UseSerilog((context, services, config) =>
        {
            var updatedConfig = config
                .Enrich.WithThreadId()
                .Enrich.FromLogContext()
                .ReadFrom.Services(services)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information);

            ConfigureLogs(updatedConfig, useJsonFormatOnFiles, useJsonFormatOnConsole, logToFile, logsPath, logs);
        }, true);
    }

    public static ILoggingBuilder ConfigureAppLogging(
        this ILoggingBuilder loggingBuilder,
        string? logsPath = null,
        bool useJsonFormatOnFiles = false,
        bool useJsonFormatOnConsole = false,
        params ToLog[] logs)
    {
        bool logToFile = !string.IsNullOrWhiteSpace(logsPath);
        if (logToFile)
        {
            TryCreateLogFolder(logsPath!);
        }

        var config = new LoggerConfiguration()
            .Enrich.WithThreadId()
            .Enrich.FromLogContext()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information);

        ConfigureLogs(config, useJsonFormatOnFiles, useJsonFormatOnConsole, logToFile, logsPath, logs);
        loggingBuilder.AddSerilog(config.CreateLogger());
        return loggingBuilder;
    }

    private static void ConfigureLogs(
        LoggerConfiguration config,
        bool useJsonFormatOnFiles,
        bool useJsonFormatOnConsole,
        bool logToFile,
        string? logsPath,
        params ToLog[] logs)
    {
        foreach (ToLog log in logs)
        {
            string filename = log.LogFileName;
            if (!log.Filtered)
            {
                throw new Exception("Only filtered logs are allowed in this point");
            }

            config.WriteTo
                .Logger(l =>
                {
                    var includingOnly = l.Filter.ByIncludingOnly(Matching.FromSource(log.AssemblyFullName!));
                    includingOnly.WriteTo.Debug(outputTemplate: FullLoggingFormat);
                    if (useJsonFormatOnConsole)
                    {
                        includingOnly.WriteTo.Console(new JsonFormatter());
                    }
                    else
                    {
                        includingOnly.WriteTo.Console(outputTemplate: FullLoggingFormat);
                    }

                    if (!logToFile)
                    {
                        return;
                    }

                    if (useJsonFormatOnFiles)
                    {
                        includingOnly.WriteTo.File(
                            new JsonFormatter(),
                            Path.Combine(logsPath!, filename),
                            rollingInterval: RollingInterval.Day,
                            fileSizeLimitBytes: TenMb,
                            rollOnFileSizeLimit: true);
                    }
                    else
                    {
                        includingOnly.WriteTo.File(
                            Path.Combine(logsPath!, filename),
                            rollingInterval: RollingInterval.Day,
                            fileSizeLimitBytes: TenMb,
                            rollOnFileSizeLimit: true,
                            outputTemplate: FullLoggingFormat);
                    }
                });
        }
    }
}