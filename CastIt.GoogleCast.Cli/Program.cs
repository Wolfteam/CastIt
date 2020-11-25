using CastIt.Application;
using CastIt.GoogleCast.Cli.Commands;
using CastIt.GoogleCast.Cli.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CastIt.GoogleCast.Cli
{
    public static class Program
    {
        private static async Task<int> Main(string[] args)
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
                }).ConfigureAppConfiguration(b => b.AddJsonFile("appsettings.json"));

            try
            {
                return await builder.RunCommandLineApplicationAsync<MainCommand>(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException?.Message);
                return 1;
            }
        }
    }
}
