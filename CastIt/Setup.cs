using CastIt.Common.Miscellaneous;
using Microsoft.Extensions.Logging;
using MvvmCross.Platforms.Wpf.Core;
using MvvmCross.Platforms.Wpf.Presenters;
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
            return new SerilogLoggerFactory();
        }
    }
}
