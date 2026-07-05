using CastIt.Common.Miscellaneous;
using CastIt.Domain.Utils;
using CastIt.Shared.Extensions;
using Microsoft.Extensions.Logging;
using MvvmCross.Platforms.Wpf.Core;
using MvvmCross.Platforms.Wpf.Presenters;
using Serilog;
using Serilog.Extensions.Logging;
using System.Windows.Controls;

namespace CastIt
{
    public class Setup : MvxWpfSetup<SetupApplication>
    {
        protected override IMvxWpfViewPresenter CreateViewPresenter(ContentControl root)
            => new CustomAppPresenter(root);

        protected override ILoggerProvider CreateLogProvider()
        {
            return new SerilogLoggerProvider();
        }

        protected override ILoggerFactory CreateLogFactory()
        {
            string basePath = AppFileUtils.GetDesktopLogsPath();
            var config = LoggingExtensions.CreateLoggerConfiguration("CastIt", logsPath: basePath);
            Log.Logger = config.CreateLogger();
            return new SerilogLoggerFactory();
        }
    }
}
