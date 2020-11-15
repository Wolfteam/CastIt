using CastIt.Interfaces;
using CastIt.Server.Interfaces;
using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;

namespace CastIt.ViewModels.Dialogs
{
    public class AboutDialogViewModel : BaseDialogViewModel
    {
        private readonly IMvxNavigationService _navigationService;
        private readonly IAppWebServer _appWebServer;

        public string CastItServerUrl
            => GetText("ServerUrl", _appWebServer.BaseUrl);

        public AboutDialogViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            IMvxLogProvider logger,
            IMvxNavigationService navigationService,
            IAppWebServer appWebServer)
            : base(textProvider, messenger, logger.GetLogFor<AboutDialogViewModel>())
        {
            _navigationService = navigationService;
            _appWebServer = appWebServer;
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
