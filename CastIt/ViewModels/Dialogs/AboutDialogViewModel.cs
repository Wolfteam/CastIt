using CastIt.Interfaces;
using Microsoft.Extensions.Logging;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;

namespace CastIt.ViewModels.Dialogs
{
    public class AboutDialogViewModel : BaseDialogViewModel
    {
        private readonly IMvxNavigationService _navigationService;
        private readonly ICastItHubClientService _castItHubClientService;

        public string CastItServerUrl
            => GetText("ServerUrl", _castItHubClientService.IpAddress ?? "N/A");

        public AboutDialogViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger<AboutDialogViewModel> logger,
            IMvxNavigationService navigationService,
            ICastItHubClientService castItHubClientService)
            : base(textProvider, messenger, logger)
        {
            _navigationService = navigationService;
            _castItHubClientService = castItHubClientService;
        }

        public override void Prepare()
        {
            base.Prepare();

            Title = GetText("About");
        }

        public override void SetCommands()
        {
            base.SetCommands();
            OkCommand = new MvxAsyncCommand(async () => await _navigationService.Close(this));
        }
    }
}
