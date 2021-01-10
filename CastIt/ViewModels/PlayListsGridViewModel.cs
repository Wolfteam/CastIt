using CastIt.Interfaces;
using Microsoft.Extensions.Logging;
using MvvmCross.Plugin.Messenger;

namespace CastIt.ViewModels
{
    public class PlayListsGridViewModel : BaseViewModel
    {
        public PlayListsGridViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger<PlayListsGridViewModel> logger)
            : base(textProvider, messenger, logger)
        {
        }
    }
}
