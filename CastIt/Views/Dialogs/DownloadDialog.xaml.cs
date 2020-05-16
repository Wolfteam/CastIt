using CastIt.ViewModels.Dialogs;
using MvvmCross.Platforms.Wpf.Presenters.Attributes;
using MvvmCross.Platforms.Wpf.Views;
using MvvmCross.ViewModels;

namespace CastIt.Views.Dialogs
{
    [MvxViewFor(typeof(DownloadDialogViewModel))]
    [MvxWindowPresentation(Identifier = nameof(DownloadDialog), Modal = true)]
    public partial class DownloadDialog : MvxWindow
    {
        public DownloadDialog()
        {
            InitializeComponent();
        }
    }
}
