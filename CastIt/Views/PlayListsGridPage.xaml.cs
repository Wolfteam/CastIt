using CastIt.Common.Miscellaneous;
using CastIt.ViewModels;
using MvvmCross.Platforms.Wpf.Views;

namespace CastIt.Views
{
    [CustomMvxContentPresentation(typeof(PlayListsGridViewModel), true, NoHistory = true)]
    public partial class PlayListsGridPage : MvxWpfView<PlayListsGridViewModel>
    {
        public PlayListsGridPage()
        {
            InitializeComponent();
        }
    }
}
