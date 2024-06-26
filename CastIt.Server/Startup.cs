using CastIt.FFmpeg;
using CastIt.GoogleCast;
using CastIt.GoogleCast.Interfaces;
using CastIt.GoogleCast.LocalFile;
using CastIt.GoogleCast.Youtube;
using CastIt.Server.Common;
using CastIt.Server.Common.Extensions;
using CastIt.Server.FileWatcher;
using CastIt.Server.Hubs;
using CastIt.Server.Interfaces;
using CastIt.Server.Middleware;
using CastIt.Server.Services;
using CastIt.Server.Shared;
using CastIt.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;
using System;
using System.Linq;

namespace CastIt.Server
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(b =>
            {
                b.AddDebug();
                b.AddConsole();
                b.AddSerilog();
                b.AddEventLog();
            });

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
            services.AddSignalR(o =>
            {
                o.EnableDetailedErrors = true;
            });
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
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            app.UseHttpsRedirection();
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

            //app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<CastItHub>("/CastItHub");
            });

            app.UseSwagger("CastIt");

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:3000");
                    //spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }
    }
}
