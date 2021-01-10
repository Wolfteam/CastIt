using CastIt.Interfaces;
using Microsoft.Extensions.Logging;
using MvvmCross.Plugin.Messenger;

namespace CastIt.ViewModels.Items
{
    public class PlayListGridItemViewModel : BaseViewModel<PlayListItemViewModel>
    {
        public PlayListItemViewModel Item { get; private set; }

        public PlayListGridItemViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger<PlayListGridItemViewModel> logger)
            : base(textProvider, messenger, logger)
        {
        }

        public override void Prepare(PlayListItemViewModel parameter)
        {
            Item = parameter;
        }
    }
}
