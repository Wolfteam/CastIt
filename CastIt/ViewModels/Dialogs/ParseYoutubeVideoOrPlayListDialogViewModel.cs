using CastIt.Interfaces;
using CastIt.Models;
using Microsoft.Extensions.Logging;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using System.Threading.Tasks;

namespace CastIt.ViewModels.Dialogs
{
    public class ParseYoutubeVideoOrPlayListDialogViewModel : BaseDialogViewModelResult<NavigationBoolResult>
    {
        private readonly IMvxNavigationService _navigationService;
        private string _secondaryOkText;

        public string SecondaryOKText
        {
            get => _secondaryOkText;
            set => SetProperty(ref _secondaryOkText, value);
        }

        public IMvxAsyncCommand SecondaryOkCommand { get; private set; }

        public ParseYoutubeVideoOrPlayListDialogViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger<ParseYoutubeVideoOrPlayListDialogViewModel> logger,
            IMvxNavigationService navigationService)
            : base(textProvider, messenger, logger)
        {
            _navigationService = navigationService;
        }

        public override void Prepare()
        {
            base.Prepare();
            Title = GetText("Confirmation");
            OkText = GetText("Video");
            SecondaryOKText = GetText("PlayList");
            ContentText = GetText("ParseVideoOrPlayList");
        }

        public override void SetCommands()
        {
            base.SetCommands();
            OkCommand = new MvxAsyncCommand(async () => await CloseDialog(true));
            SecondaryOkCommand = new MvxAsyncCommand(async () => await CloseDialog(false));
            CloseCommand = new MvxAsyncCommand(async () => await CloseDialog(null));
        }

        private Task CloseDialog(bool? parseVideo)
            => _navigationService.Close(this, !parseVideo.HasValue ? null : new NavigationBoolResult(parseVideo.Value));
    }
}
