using CastIt.Interfaces;
using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;

namespace CastIt.ViewModels.Dialogs
{
    public class AboutDialogViewModel : BaseDialogViewModel
    {
        private readonly IMvxNavigationService _navigationService;

        public AboutDialogViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            IMvxLogProvider logger,
            IMvxNavigationService navigationService)
            : base(textProvider, messenger, logger.GetLogFor<AboutDialogViewModel>())
        {
            _navigationService = navigationService;
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
