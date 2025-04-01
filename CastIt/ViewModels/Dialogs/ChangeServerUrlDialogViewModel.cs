using CastIt.Interfaces;
using CastIt.Models.Results;
using Microsoft.Extensions.Logging;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels.Result;
using System;
using System.Threading.Tasks;

namespace CastIt.ViewModels.Dialogs
{
    public class ChangeServerUrlDialogViewModel : BaseDialogViewModelResult<NavigationBoolResult>
    {
        private readonly IMvxNavigationService _navigationService;
        private readonly ICastItHubClientService _castItHub;
        private readonly IDesktopAppSettingsService _desktopAppSettings;

        private bool _isBusy;
        private string _newServerIpAddress;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public bool IsNewServerIpAddressValid
            => !string.IsNullOrWhiteSpace(NewServerIpAddress) && Uri.TryCreate(NewServerIpAddress, UriKind.Absolute, out _);

        public string NewServerIpAddress
        {
            get => _newServerIpAddress;
            set
            {
                SetProperty(ref _newServerIpAddress, value);
                RaisePropertyChanged(() => IsNewServerIpAddressValid);
            }
        }

        public string CurrentServerIpAddress
            => _desktopAppSettings.ServerUrl;

        public IMvxAsyncCommand<string> SaveUrlCommand { get; private set; }

        public ChangeServerUrlDialogViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger<ChangeServerUrlDialogViewModel> logger,
            IMvxNavigationService navigationService,
            ICastItHubClientService castItHub,
            IMvxResultViewModelManager resultViewModelManager,
            IDesktopAppSettingsService desktopAppSettings)
            : base(textProvider, messenger, logger, resultViewModelManager)
        {
            _castItHub = castItHub;
            _navigationService = navigationService;
            _desktopAppSettings = desktopAppSettings;
        }

        public override Task Initialize()
        {
            Title = GetText("ChangeServerUrl");
            NewServerIpAddress = CurrentServerIpAddress;
            return base.Initialize();
        }

        public override void SetCommands()
        {
            base.SetCommands();

            SaveUrlCommand = new MvxAsyncCommand<string>(ChangeServerUrl);

            CloseCommand = new MvxAsyncCommand(async () => await _navigationService.CloseSettingResult(this, NavigationBoolResult.Fail()));
        }

        private async Task ChangeServerUrl(string url)
        {
            if (!IsNewServerIpAddressValid)
            {
                return;
            }

            IsBusy = true;
            ContentText = null;
            try
            {
                bool connected = await _castItHub.Init(url).ConfigureAwait(false);
                if (!connected)
                {
                    ContentText = GetText("ConnectionCouldNotBeEstablished");
                    return;
                }

                _desktopAppSettings.ServerUrl = url;
                await _navigationService.CloseSettingResult(this, NavigationBoolResult.Succeed());
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"{nameof(ChangeServerUrl)}: Unknown error");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
