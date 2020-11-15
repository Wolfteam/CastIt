using CastIt.Common.Miscellaneous;
using MvvmCross.Platforms.Wpf.Core;
using MvvmCross.Platforms.Wpf.Presenters;
using System.Windows.Controls;

namespace CastIt
{
    public class Setup : MvxWpfSetup<SetupApplication>
    {
        protected override IMvxWpfViewPresenter CreateViewPresenter(ContentControl root)
            => new CustomAppPresenter(root);
    }
}
