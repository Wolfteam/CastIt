using CastIt.ViewModels.Dialogs;
using MvvmCross.Platforms.Wpf.Presenters.Attributes;
using MvvmCross.Platforms.Wpf.Views;
using MvvmCross.ViewModels;

namespace CastIt.Views.Dialogs
{
    [MvxViewFor(typeof(AddEditEndpointDialogViewModel))]
    [MvxWindowPresentation(Identifier = nameof(AddEditEndpointDialog), Modal = true)]
    public partial class AddEditEndpointDialog : MvxWindow<AddEditEndpointDialogViewModel>
    {
        public AddEditEndpointDialog()
        {
            InitializeComponent();
        }
    }
}
