using CastIt.Interfaces;
using Microsoft.Extensions.Logging;
using MvvmCross.Plugin.Messenger;

namespace CastIt.ViewModels
{
    public class PlayListsViewModel : BaseViewModel
    {
        public PlayListsViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger<PlayListsViewModel> logger)
            : base(textProvider, messenger, logger)
        {
        }
    }
}
