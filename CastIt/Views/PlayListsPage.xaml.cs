using CastIt.Common.Miscellaneous;
using CastIt.ViewModels;
using MvvmCross.Platforms.Wpf.Views;

namespace CastIt.Views
{
    [CustomMvxContentPresentation(typeof(PlayListsViewModel), true, NoHistory = true)]
    public partial class PlayListsPage : MvxWpfView<PlayListsViewModel>
    {
        public PlayListsPage()
        {
            InitializeComponent();
        }
    }
}
