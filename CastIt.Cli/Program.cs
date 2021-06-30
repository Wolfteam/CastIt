using CastIt.Application;
using CastIt.Cli.Commands;
using CastIt.Cli.Common.Utils;
using CastIt.Cli.Interfaces.Api;
using CastIt.Cli.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Refit;
using Serilog;
using Serilog.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using CastIt.Cli.Services;

namespace CastIt.Cli
{
    public static class Program
    {
        private static async Task<int> Main(string[] args)
        {
            //This is required, so calls to Directory.GetCurrentDirectory return the right path and calls like 'castit --start-server' works properly
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            Directory.SetCurrentDirectory(basePath);
            var jsonPath = Path.Combine(basePath, "appsettings.json");
            try
            {
                Log.Logger = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .CreateLogger();

                var builder = new HostBuilder()
                    .ConfigureServices((hostContext, services) =>
                    {
                        var config = hostContext.Configuration;
                        var appSettings = config.GetSection(nameof(AppSettings)).Get<AppSettings>();
                        if (appSettings == null)
                            throw new NullReferenceException("App Settings should not be null, but was");

                        services.AddSingleton(appSettings);

                        services.AddApplicationForCli();
                        services.AddLogging(c =>
                        {
                            c.ClearProviders();
                            c.AddProvider(new SerilogLoggerProvider(Log.Logger));
                        });

                        services.AddSingleton<ICastItApiService, CastItApiService>();

                        var url = ServerUtils.StartServerIfNotStarted();
#if DEBUG
                        Console.WriteLine($"Using url = {url} for the web server api");
#endif
                        services.AddRefitClient<ICastItApi>()
                            .ConfigureHttpClient(c => c.BaseAddress = new Uri(url));
                    }).ConfigureAppConfiguration(b =>
                    {
                        b.SetBasePath(basePath);
                        b.AddJsonFile(jsonPath);
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
}
