using CastIt.Application;
using CastIt.Application.Interfaces;
using CastIt.Infrastructure;
using CastIt.Test.Common;
using CastIt.Test.Common.Extensions;
using CastIt.Test.Interfaces;
using CastIt.Test.Middleware;
using CastIt.Test.Models;
using CastIt.Test.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;

namespace CastIt.Test
{
    public class Startup
    {
        private readonly string _ffmpegPath;
        private readonly string _ffprobePath;

        public IConfiguration Configuration { get; }

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

            var defaultSettings = Configuration.GetSection(nameof(AppSettings)).Get<AppSettings>();

            if (!string.IsNullOrWhiteSpace(_ffmpegPath))
            {
                defaultSettings.FFmpegPath = _ffmpegPath;
            }

            if (!string.IsNullOrWhiteSpace(_ffprobePath))
            {
                defaultSettings.FFprobePath = _ffprobePath;
            }

            services.AddApplication(defaultSettings.FFmpegPath, defaultSettings.FFprobePath)
                .AddServerInfrastructure();
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
            services.AddSingleton<IServerCastService, NewCastService>();
            services.AddSingleton<IBaseWebServer, FakeAppWebServer>();
            services.AddAutoMapper(config => config.AddProfile(typeof(MappingProfile)));
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

            app.UseMiddleware<ExceptionHandlerMiddleware>();

            //app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<CastItHub>("/CastitHub");
            });
            app.UseSwagger("CastIt");

            //Since the hosted service is started after this, we need to make sure that this thing is initialized
            using var scope = app.ApplicationServices.CreateScope();
            var castService = scope.ServiceProvider.GetRequiredService<IServerCastService>();
            castService.Init().GetAwaiter().GetResult();
        }
    }
}
