using CastIt.Interfaces;
using CastIt.Models.Results;
using Microsoft.Extensions.Logging;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels.Result;
using System.Threading.Tasks;

namespace CastIt.ViewModels.Dialogs
{
    public class ParseYoutubeVideoOrPlayListDialogViewModel : BaseDialogViewModelResult<string, OnYoutubeUrlAddedResult>
    {
        private readonly IMvxNavigationService _navigationService;
        private string _parameter;

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
            IMvxNavigationService navigationService,
            IMvxResultViewModelManager resultViewModelManager)
            : base(textProvider, messenger, logger, resultViewModelManager)
        {
            _navigationService = navigationService;
        }

        public override void Prepare(string parameter)
        {
            _parameter = parameter;
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

        private Task<bool> CloseDialog(bool? parseVideo)
        {
            OnYoutubeUrlAddedResult result = new OnYoutubeUrlAddedResult(parseVideo ?? false, !parseVideo.HasValue, _parameter);
            return _navigationService.CloseSettingResult(this, result);
        }
    }
}
