using CastIt.ViewModels.Dialogs;
using MvvmCross.Platforms.Wpf.Presenters.Attributes;
using MvvmCross.Platforms.Wpf.Views;
using MvvmCross.ViewModels;
using System.Diagnostics;
using System.Windows.Navigation;

namespace CastIt.Views.Dialogs
{
    [MvxViewFor(typeof(AboutDialogViewModel))]
    [MvxWindowPresentation(Identifier = nameof(DownloadDialog), Modal = true)]
    public partial class AboutDialog : MvxWindow
    {
        public AboutDialog()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri)
            {
                UseShellExecute = true
            });
            e.Handled = true;
        }
    }
}
