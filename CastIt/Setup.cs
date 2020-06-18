using CastIt.Common.Miscellaneous;
using CastIt.Common.Utils;
using CastIt.GoogleCast;
using CastIt.Server;
using CastIt.Services;
using CastIt.ViewModels;
using CastIt.ViewModels.Dialogs;
using CastIt.ViewModels.Items;
using MvvmCross.Logging;
using MvvmCross.Platforms.Wpf.Core;
using MvvmCross.Platforms.Wpf.Presenters;
using Serilog;
using Serilog.Filters;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;

namespace CastIt
{
    public class Setup : MvxWpfSetup<Application>
    {
        protected override IMvxWpfViewPresenter CreateViewPresenter(ContentControl root)
            => new CustomAppPresenter(root);

        public override MvxLogProviderType GetDefaultLogProviderType()
            => MvxLogProviderType.Serilog;

        protected override IMvxLogProvider CreateLogProvider()
        {
            const string fileOutputTemplate =
                "{Timestamp:dd-MM-yyyy HH:mm:ss.fff} [{Level}] {Message:lj}{NewLine}{Exception}";
            var basePath = FileUtils.GetLogsPath();
            var loggerConfig = new LoggerConfiguration().MinimumLevel
                .Debug();

            var logs = new Dictionary<string, string>
            {
                {$"{typeof(MainViewModel).FullName}",  "vm_main_.txt"},
                {$"{typeof(DevicesViewModel).FullName}",  "vm_devices_.txt"},
                {$"{typeof(SettingsViewModel).FullName}",  "vm_settings_.txt"},
                {$"{typeof(PlayListItemViewModel).FullName}",  "vm_playlistitem_.txt"},
                {$"{typeof(FileItemViewModel).FullName}",  "vm_fileitem_.txt"},
                {$"{typeof(DeviceItemViewModel).FullName}",  "vm_deviceitem_.txt"},
                {$"{typeof(DownloadDialogViewModel).FullName}",  "vm_download_dialog_.txt"},
                {$"{typeof(CastService).FullName}",  "service_cast_.txt"},
                {$"{typeof(AppSettingsService).FullName}",  "service_appsettings_.txt"},
                {$"{typeof(FFMpegService).FullName}",  "service_ffmpeg_.txt"},
                {$"{typeof(Player).FullName}",  "googlecast_player_.txt"},
                {$"{typeof(VideoModule).FullName}",  "server_video_.txt"},
                {$"{typeof(YoutubeUrlDecoder).FullName}",  "decoder_youtube_.txt"},
            };

            foreach (var kvp in logs)
            {
                loggerConfig.WriteTo
                    .Logger(l =>
                        l.Filter.ByIncludingOnly(Matching.FromSource(kvp.Key))
                        .WriteTo.File(
                            Path.Combine(basePath, kvp.Value),
                            rollingInterval: RollingInterval.Day,
                            rollOnFileSizeLimit: true,
                            outputTemplate: fileOutputTemplate)
                        .WriteTo.Debug(outputTemplate: fileOutputTemplate)
                    );
            }
            Log.Logger = loggerConfig.CreateLogger();
            return base.CreateLogProvider();
        }
    }
}
