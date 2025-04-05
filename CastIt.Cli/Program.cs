using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CastIt.Cli.Commands;
using CastIt.Cli.Interfaces.Api;
using CastIt.Cli.Models;
using CastIt.Cli.Services;
using CastIt.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;

namespace CastIt.Cli;

public static class Program
{
    private static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        //This is required, so calls to Directory.GetCurrentDirectory return the right path and calls like 'castit --start-server' works properly
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        Directory.SetCurrentDirectory(basePath);
        try
        {
            AppSettings appSettings = await AppSettings.Get();

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .CreateLogger();

            var builder = new HostBuilder()
                .ConfigureServices((_, services) =>
                {
                    services.AddSingleton(appSettings);
                    services.AddTelemetry().AddCommonFileService();
                    services.AddLogging(c =>
                    {
                        c.ClearProviders();
                        c.AddProvider(new SerilogLoggerProvider(Log.Logger)).AddDebug().AddConsole();
                    });

                    services.AddSingleton<ICastItApiService, CastItApiService>();

                }).ConfigureAppConfiguration(b =>
                {
                    b.SetBasePath(basePath);
                });

            return await builder.RunCommandLineApplicationAsync<MainCommand>(args);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Something went wrong!. Current basePath = {basePath}");
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.InnerException?.Message);
            Console.WriteLine(ex.StackTrace);
            return 1;
        }
    }
}