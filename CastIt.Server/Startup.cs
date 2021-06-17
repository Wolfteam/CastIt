using CastIt.Application;
using CastIt.Application.Interfaces;
using CastIt.GoogleCast;
using CastIt.GoogleCast.Interfaces;
using CastIt.Infrastructure.Models;
using CastIt.Server.Common;
using CastIt.Server.Common.Extensions;
using CastIt.Server.Hubs;
using CastIt.Server.Interfaces;
using CastIt.Server.Middleware;
using CastIt.Server.Services;
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

namespace CastIt.Server
{
    public class Startup
    {
        private readonly string _ffmpegPath;
        private readonly string _ffprobePath;

        public IConfiguration Configuration { get; }

        //TODO: REMOVE FFMPEG FROM HERE. LOAD FFMPEG PATH FROM THE APPSETTINGS
        public Startup(IConfiguration configuration, string ffmpegPath, string ffprobePath)
        {
            Configuration = configuration;
            _ffmpegPath = ffmpegPath;
            _ffprobePath = ffprobePath;
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

            var defaultSettings = Configuration.GetSection(nameof(ServerAppSettings)).Get<ServerAppSettings>();

            if (!string.IsNullOrWhiteSpace(_ffmpegPath))
            {
                defaultSettings.FFmpegPath = _ffmpegPath;
            }

            if (!string.IsNullOrWhiteSpace(_ffprobePath))
            {
                defaultSettings.FFprobePath = _ffprobePath;
            }

            //Cors is required for the subtitles to work
            services.AddCors();

            //Should be more than enough for the hosted service to complete
            services.Configure<HostOptions>(opts => opts.ShutdownTimeout = TimeSpan.FromSeconds(3));

            services.AddApplication(defaultSettings.FFmpegPath, defaultSettings.FFprobePath);
            services.AddSingleton(defaultSettings);
            services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    //TODO: NOT SURE IF STRING ENUM CONVERTER IS A GOOD OPTION
                    //TODO: SHOULD I SWITCH TO System.Text.Json ?
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.Formatting = Formatting.Indented;
                });
            services.AddSignalR();
            services.AddSingleton<IAppDataService, AppDataService>();
            services.AddSingleton<IServerCastService, ServerCastService>();
            services.AddSingleton<IBaseWebServer, FakeAppWebServer>();
            services.AddSingleton<IServerAppSettingsService, ServerAppSettingsService>();
            services.AddSingleton<IBaseWebServer, FakeAppWebServer>();
            services.AddSingleton<IPlayer>(provider => new Player(provider.GetRequiredService<ILogger<Player>>()));
            services.AddAutoMapper(config => config.AddProfile(typeof(MappingProfile)));
            services.AddSingleton<IImageProviderService, ImageProviderService>();
            services.AddSwagger("CastIt", "CastIt.xml");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

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

            //Since the hosted service is started after this, we need to make sure that this thing is initialized
            //using var scope = app.ApplicationServices.CreateScope();
            //var castService = scope.ServiceProvider.GetRequiredService<IServerCastService>();
            //castService.Init().GetAwaiter().GetResult();
        }
    }
}
