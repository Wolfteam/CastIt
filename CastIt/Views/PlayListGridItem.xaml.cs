using CastIt.Common.Miscellaneous;
using CastIt.ViewModels.Items;
using MvvmCross.Platforms.Wpf.Views;

namespace CastIt.Views
{
    [CustomMvxContentPresentation(typeof(PlayListGridItemViewModel), true, NoHistory = true)]
    public partial class PlayListGridItem : MvxWpfView<PlayListGridItemViewModel>
    {
        public PlayListGridItem()
        {
            InitializeComponent();
        }
    }
}
