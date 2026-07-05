using System;
using System.Collections.Generic;
using System.IO;
using CastIt.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using ILogger = Serilog.ILogger;

namespace CastIt.Shared.Extensions;

public static class LoggingExtensions
{
    public const string SimpleLoggingFormat = "({ThreadId}) [{Level}] {Message:lj}{NewLine}{Exception}";

    public const string FullLoggingFormat = "{Timestamp:dd-MM-yyyy HH:mm:ss.fff} " + SimpleLoggingFormat;

    public const int TenMb = 10_000_000;

    public static ILogger CreateBootstrapperLogger(string applicationName, string? logsPath = null)
    {
        if (!string.IsNullOrWhiteSpace(logsPath))
        {
            TryCreateLogFolder(logsPath);
        }

        var config = new LoggerConfiguration()
            .Enrich.WithThreadId()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", applicationName)
            .WriteTo.Debug(outputTemplate: FullLoggingFormat)
            .WriteTo.Console(new JsonFormatter());

        if (!string.IsNullOrWhiteSpace(logsPath))
        {
            WriteToFile(config, applicationName, logsPath);
        }

        return config.CreateBootstrapLogger();
    }

    public static IServiceCollection AddCastItLogging(
        this IServiceCollection services,
        IConfiguration configuration,
        string applicationName,
        IReadOnlyDictionary<string, string>? customProperties = null,
        string? logsPath = null)
    {
        logsPath = ResolveLogsPath(logsPath);

        services.AddLogging();

        return services.AddSerilog((sp, config) =>
        {
            ConfigureBaseLogger(config, applicationName, customProperties);

            config.ReadFrom.Services(sp);
            config.ReadFrom.Configuration(configuration);
            config.WriteTo.Debug(outputTemplate: FullLoggingFormat);
            config.WriteTo.Console(new JsonFormatter());

            if (!string.IsNullOrWhiteSpace(logsPath))
            {
                WriteToFile(config, applicationName, logsPath);
            }
        }, true);
    }

    public static LoggerConfiguration CreateLoggerConfiguration(
        string applicationName,
        IReadOnlyDictionary<string, string>? customProperties = null,
        string? logsPath = null)
    {
        logsPath = ResolveLogsPath(logsPath);

        var config = new LoggerConfiguration();
        ConfigureBaseLogger(config, applicationName, customProperties);
        config.WriteTo.Debug(outputTemplate: FullLoggingFormat);
        config.WriteTo.Console(new JsonFormatter());

        if (!string.IsNullOrWhiteSpace(logsPath))
        {
            WriteToFile(config, applicationName, logsPath);
        }

        return config;
    }

    // Central log-level policy for every CastIt host. Keeping it here means all hosts behave the
    // same regardless of appsettings drift. It can still be overridden per-host via a "Serilog"
    // configuration section, which ReadFrom.Configuration applies in AddCastItLogging.
    private static void ConfigureBaseLogger(
        LoggerConfiguration config,
        string applicationName,
        IReadOnlyDictionary<string, string>? customProperties)
    {
        config
            .Enrich.WithThreadId()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", applicationName);

        config.MinimumLevel.Information()
            // Quiet framework noise while keeping the Microsoft.Hosting.Lifetime startup banner.
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            // Quiet IHttpClientFactory's per-request "Sending/Received HTTP ..." logs.
            .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning);

        if (customProperties != null)
        {
            foreach ((string key, string value) in customProperties)
            {
                config.Enrich.WithProperty(key, value);
            }
        }
    }

    private static void WriteToFile(LoggerConfiguration config, string applicationName, string logsPath)
    {
        config.WriteTo.File(
            Path.Combine(logsPath, $"{applicationName}-.txt"),
            rollingInterval: RollingInterval.Day,
            fileSizeLimitBytes: TenMb,
            rollOnFileSizeLimit: true,
            outputTemplate: FullLoggingFormat);
    }

    private static string? ResolveLogsPath(string? logsPath)
    {
        logsPath ??= AppWebServerConstants.InDocker
            ? null
            : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

        if (!string.IsNullOrWhiteSpace(logsPath))
        {
            TryCreateLogFolder(logsPath);
        }

        return logsPath;
    }

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
}
