using CastIt.ViewModels.Dialogs;
using MvvmCross.Platforms.Wpf.Presenters.Attributes;
using MvvmCross.Platforms.Wpf.Views;
using MvvmCross.ViewModels;

namespace CastIt.Views.Dialogs
{
    [MvxViewFor(typeof(ChangeServerUrlDialogViewModel))]
    [MvxWindowPresentation(Identifier = nameof(ChangeServerUrlDialog), Modal = true)]
    public partial class ChangeServerUrlDialog : MvxWindow<ChangeServerUrlDialogViewModel>
    {
        public ChangeServerUrlDialog()
        {
            InitializeComponent();
        }
    }
}
