using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CastIt.Domain;
using CastIt.FFmpeg;
using CastIt.GoogleCast;
using CastIt.GoogleCast.Interfaces;
using CastIt.GoogleCast.LocalFile;
using CastIt.GoogleCast.Youtube;
using CastIt.Server.Common;
using CastIt.Server.Common.Extensions;
using CastIt.Server.Controllers;
using CastIt.Server.FileWatcher;
using CastIt.Server.Hubs;
using CastIt.Server.Interfaces;
using CastIt.Server.Middleware;
using CastIt.Server.Services;
using CastIt.Server.Shared;
using CastIt.Shared;
using CastIt.Shared.Extensions;
using CastIt.Shared.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;

bool logToFiles = !AppWebServerConstants.InDocker;
string? logsPath = logToFiles ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs") : null;

Log.Logger = LoggingExtensions.CreateBootstrapperLogger(new ToLog(nameof(Program)), logsPath);

try
{
    Log.Information("Creating builder...");
    var builder = WebApplication.CreateBuilder(args);

    Log.Information("Configuring logs...");
    var logs = ToLog.From<BaseController>()
        .Concat(new List<ToLog>
        {
            new ToLog(typeof(ExceptionHandlerMiddleware))
        })
        .Concat(ToLog.From(typeof(AppDataService)))
        .Concat(ToLog.From(typeof(CastItHub)))
        .Concat(ToLog.From(typeof(CastIt.FFmpeg.DependencyInjection)))
        .Concat(ToLog.From(typeof(CastIt.GoogleCast.DependencyInjection)))
        .Concat(ToLog.From(typeof(CastIt.GoogleCast.LocalFile.DependencyInjection)))
        .Concat(ToLog.From(typeof(CastIt.GoogleCast.Youtube.DependencyInjection)))
        .Concat(ToLog.From(typeof(CastIt.Youtube.DependencyInjection)))
        .ToArray();
    builder.Host.ConfigureAppLogging(logsPath, false, false, logs);

    Log.Information("Configuring services...");
    IServiceCollection services = builder.Services;
    //Cors is required for the subtitles to work
    services.AddCors();

    //Should be more than enough for the hosted service to complete
    services.Configure<HostOptions>(opts => opts.ShutdownTimeout = TimeSpan.FromSeconds(15));

    services.AddFileService().AddTelemetry();
    services.AddControllers()
        .AddNewtonsoftJson(options =>
        {
            //TODO: NOT SURE IF STRING ENUM CONVERTER IS A GOOD OPTION
            //TODO: SHOULD I SWITCH TO System.Text.Json ?
            options.SerializerSettings.Converters.Add(new StringEnumConverter());
            options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            options.SerializerSettings.Formatting = Formatting.Indented;
        });
    services.AddSignalR(o => { o.EnableDetailedErrors = true; });
    services.AddSingleton<IAppDataService, AppDataService>();
    services.AddSingleton<IServerCastService, ServerCastService>();
    services.AddSingleton<IServerAppSettingsService, ServerAppSettingsService>();
    services.AddSingleton<IServerService, ServerService>();
    services.AddSingleton<IImageProviderService, ImageProviderService>();
    services.AddSingleton<IFileWatcherService, FileWatcherService>();
    services.AddFFmpeg()
        .AddGoogleCast()
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

    // In production, the React files will be served from this directory
    services.AddSpaStaticFiles(configuration => configuration.RootPath = "ClientApp/dist");

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

    app.UseStaticFiles();
    app.UseSpaStaticFiles();
    app.UseRouting();

    //Cors is required for the subtitles to work
    app.UseCors(options =>
        options.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
    );
    app.UseMiddleware<ExceptionHandlerMiddleware>();

    app.MapControllers();
    app.MapHub<CastItHub>("/CastItHub");

    app.UseSwagger("CastIt");

    app.UseSpa(spa =>
    {
        spa.Options.SourcePath = "ClientApp";

        if (app.Environment.IsDevelopment())
        {
            spa.UseProxyToSpaDevelopmentServer("http://localhost:3000");
            //spa.UseReactDevelopmentServer(npmScript: "start");
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