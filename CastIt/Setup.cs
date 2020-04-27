using CastIt.Common.Miscellaneous;
using MvvmCross.Logging;
using MvvmCross.Platforms.Wpf.Core;
using MvvmCross.Platforms.Wpf.Presenters;
using Serilog;
using Serilog.Filters;
using System;
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
            var externalFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var basePath = Path.Combine(externalFolder, "Logs");
            CreateDirectory(externalFolder, basePath);
            var loggerConfig = new LoggerConfiguration().MinimumLevel
                .Verbose();

            var logs = new Dictionary<string, string>
            {

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
                    );
            }
            Log.Logger = loggerConfig.CreateLogger();
            return base.CreateLogProvider();
        }

        private static string CreateDirectory(string baseFolder, string folder)
        {
            var fullPath = Path.Combine(baseFolder, folder);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            return fullPath;
        }
    }
}
