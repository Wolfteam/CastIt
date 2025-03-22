using System;
using System.Collections.Generic;
using System.Linq;
using CastIt.Domain.Utils;
using CastIt.FFmpeg;
using CastIt.GoogleCast;
using CastIt.GoogleCast.Interfaces;
using CastIt.GoogleCast.LocalFile;
using CastIt.GoogleCast.Youtube;
using CastIt.Server.Common;
using CastIt.Server.Common.Extensions;
using CastIt.Server.Controllers;
using CastIt.Server.Hubs;
using CastIt.Server.Interfaces;
using CastIt.Server.Middleware;
using CastIt.Server.Services;
using CastIt.Server.Shared;
using CastIt.Shared;
using CastIt.Shared.Extensions;
using CastIt.Shared.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Quartz;
using Serilog;
using DependencyInjection = CastIt.FFmpeg.DependencyInjection;

string logsPath = AppFileUtils.GetServerLogsPath();
Log.Logger = LoggingExtensions.CreateBootstrapperLogger(new ToLog(nameof(Program)), logsPath);
try
{
    Log.Information("Creating builder...");
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseWindowsService();

    Log.Information("Configuring logs...");
    var logs = ToLog.From<BaseController>()
        .Concat(new List<ToLog>
        {
            new ToLog(typeof(ExceptionHandlerMiddleware))
        })
        .Concat(ToLog.From(typeof(AppDataService)))
        .Concat(ToLog.From(typeof(CastItHub)))
        .Concat(ToLog.From(typeof(DependencyInjection)))
        .Concat(ToLog.From(typeof(CastIt.GoogleCast.DependencyInjection)))
        .Concat(ToLog.From(typeof(CastIt.GoogleCast.LocalFile.DependencyInjection)))
        .Concat(ToLog.From(typeof(CastIt.GoogleCast.Youtube.DependencyInjection)))
        .Concat(ToLog.From(typeof(CastIt.Youtube.DependencyInjection)))
        .ToArray();
    builder.Host.ConfigureAppLogging(logsPath, false, false, logs);

    Log.Information("Configuring settings...");
    bool useDummyPlayer = builder.Configuration.GetValue<bool>("USE_DUMMY_PLAYER");

    Log.Information("Configuring services...");
    IServiceCollection services = builder.Services;
    services.AddEndpointsApiExplorer();
    services.AddHealthChecks();
    services.AddControllers()
        .AddNewtonsoftJson(options =>
        {
            //TODO: NOT SURE IF STRING ENUM CONVERTER IS A GOOD OPTION
            //TODO: SHOULD I SWITCH TO System.Text.Json ?
            options.SerializerSettings.Converters.Add(new StringEnumConverter());
            options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            options.SerializerSettings.Formatting = Formatting.Indented;
        });
    services.Configure<HostOptions>(opts => opts.ShutdownTimeout = TimeSpan.FromSeconds(15));
    services.AddSignalR(o => { o.EnableDetailedErrors = true; });
    services.AddSpaStaticFiles(configuration => configuration.RootPath = "ClientApp/dist");
    services.AddLogging(c =>
    {
        if (OperatingSystem.IsWindows())
        {
            c.AddEventLog();
        }
    });
    services.AddQuartz(options =>
    {
        options.SchedulerId = options.SchedulerName = "CastIt.Server";
        options.CheckConfiguration = true;

        DateTimeOffset startAt = DateTimeOffset.UtcNow.Add(TimeSpan.FromMinutes(2));
        options.ScheduleJob<DeleteOldFilesJob>(trigger => trigger
            .WithIdentity($"{nameof(DeleteOldFilesJob)}-trigger".ToLowerInvariant())
            .StartAt(startAt)
            .WithDailyTimeIntervalSchedule(12, IntervalUnit.Hour)
        );
        options.ScheduleJob<SavePlayListAndFileChangesJob>(trigger => trigger
            .WithIdentity($"{nameof(SavePlayListAndFileChangesJob)}-trigger".ToLowerInvariant())
            .StartAt(startAt)
            .WithDailyTimeIntervalSchedule(3, IntervalUnit.Hour)
        );
    }).AddQuartzHostedService(options =>
    {
        options.WaitForJobsToComplete = true;
    });

    services.AddSingleton<IAppDataService, AppDataService>();
    services.AddSingleton<IServerCastService, ServerCastService>();
    services.AddSingleton<IServerAppSettingsService, ServerAppSettingsService>();
    services.AddSingleton<IServerService, ServerService>();
    services.AddSingleton<IImageProviderService, ImageProviderService>();
    services.AddSingleton<IFileWatcherService, FileWatcherService>();
    services
        .AddFileService()
        .AddTelemetry()
        .AddFFmpeg()
        .AddGoogleCast(useDummyPlayer)
        .AddGoogleCastYoutube()
        .AddGoogleCastLocalFiles();
    services.AddAutoMapper(config => config.AddProfile(typeof(MappingProfile)));

    services.AddSingleton<IBaseServerService>(provider => provider.GetRequiredService<IServerService>());
    services.AddSingleton<IMediaRequestGeneratorFactory>(provider =>
    {
        var instance = new MediaRequestGeneratorFactory();
        var searchType = typeof(IPlayMediaRequestGenerator);
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => searchType.IsAssignableFrom(p) && !p.IsInterface)
            .ToList();
        foreach (var type in types)
        {
            var service = provider.GetService(type);
            if (service == null)
            {
                throw new InvalidOperationException($"The service = {type.FullName} was not registered");
            }

            var impl = service as IPlayMediaRequestGenerator;
            instance.Add(impl);
        }

        return instance;
    });

    services.AddSwagger("CastIt", "CastIt.xml");

    services.AddHostedService<CastItHostedService>();

    Log.Information("Building app...");
    var app = builder.Build();
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseHsts();
    }

    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }

    //Cors is required for the subtitles to work
    app.UseCors(options =>
        options.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
    );
    app.UseStaticFiles();
    app.UseSpaStaticFiles();
    app.UseRouting();
    //For some reason this one is required otherwise UseSpa will "eat" the maphub route
    app.UseEndpoints(_ => { });
    app.UseHealthChecks("/healthcheck");
    app.UseMiddleware<ExceptionHandlerMiddleware>();

    app.UseSwagger("CastIt");
    app.MapControllers();
    app.MapHub<CastItHub>("/CastItHub");
    app.UseSpa(spa =>
    {
        spa.Options.SourcePath = "ClientApp";
        spa.Options.DevServerPort = 3000;

        if (app.Environment.IsDevelopment())
        {
            spa.UseReactDevelopmentServer("start");
        }
    });

    Log.Information("Running app...");
    app.Run();
}
catch (Exception e)
{
    Log.Fatal(e, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}