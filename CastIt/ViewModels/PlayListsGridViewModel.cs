using CastIt.Interfaces;
using CastIt.ViewModels.Items;
using Microsoft.Extensions.Logging;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using System.Collections.Generic;

namespace CastIt.ViewModels
{
    public class PlayListsGridViewModel : BaseViewModel<List<PlayListItemViewModel>>
    {
        public MvxObservableCollection<PlayListItemViewModel> PlayLists { get; set; }
            = new MvxObservableCollection<PlayListItemViewModel>();

        public PlayListsGridViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger<PlayListsGridViewModel> logger) : base(textProvider, messenger, logger)
        {
        }

        public override void Prepare(List<PlayListItemViewModel> parameter)
        {
            PlayLists.AddRange(parameter);
        }
    }
}
