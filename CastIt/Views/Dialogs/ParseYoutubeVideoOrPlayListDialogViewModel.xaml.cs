using CastIt.ViewModels.Dialogs;
using MvvmCross.Platforms.Wpf.Presenters.Attributes;
using MvvmCross.Platforms.Wpf.Views;
using MvvmCross.ViewModels;

namespace CastIt.Views.Dialogs
{
    [MvxViewFor(typeof(ParseYoutubeVideoOrPlayListDialogViewModel))]
    [MvxWindowPresentation(Identifier = nameof(ParseYoutubeVideoOrPlayListDialog), Modal = true)]
    public partial class ParseYoutubeVideoOrPlayListDialog : MvxWindow<ParseYoutubeVideoOrPlayListDialogViewModel>
    {
        public ParseYoutubeVideoOrPlayListDialog()
        {
            InitializeComponent();
        }
    }
}
