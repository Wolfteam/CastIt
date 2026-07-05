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
    public class AddEditEndpointParameter
    {
        public long? Id { get; set; }
        public string Name { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
    }

    public class AddEditEndpointDialogViewModel
        : BaseDialogViewModelResult<AddEditEndpointParameter, NavigationBoolResult>
    {
        private readonly IMvxNavigationService _navigationService;
        private readonly ICastItHubClientService _castItHub;

        private long? _id;
        private string _name;
        private string _host;
        private int _port = 8009;
        private bool _isBusy;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public string Name
        {
            get => _name;
            set
            {
                SetProperty(ref _name, value);
                RaisePropertyChanged(() => IsValid);
            }
        }

        public string Host
        {
            get => _host;
            set
            {
                SetProperty(ref _host, value);
                RaisePropertyChanged(() => IsValid);
            }
        }

        public int Port
        {
            get => _port;
            set
            {
                SetProperty(ref _port, value);
                RaisePropertyChanged(() => IsValid);
            }
        }

        public bool IsValid
            => !string.IsNullOrWhiteSpace(Name)
            && !string.IsNullOrWhiteSpace(Host)
            && Port >= 1
            && Port <= 65535;

        public IMvxAsyncCommand SaveCommand { get; private set; }

        public AddEditEndpointDialogViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger<AddEditEndpointDialogViewModel> logger,
            IMvxNavigationService navigationService,
            ICastItHubClientService castItHub,
            IMvxResultViewModelManager resultViewModelManager)
            : base(textProvider, messenger, logger, resultViewModelManager)
        {
            _navigationService = navigationService;
            _castItHub = castItHub;
        }

        public override void Prepare(AddEditEndpointParameter parameter)
        {
            _id = parameter?.Id;
            if (parameter != null)
            {
                Name = parameter.Name;
                Host = parameter.Host;
                if (parameter.Port > 0)
                    Port = parameter.Port;
            }
        }

        public override Task Initialize()
        {
            Title = _id.HasValue ? GetText("EditEndpoint") : GetText("AddEndpoint");
            return base.Initialize();
        }

        public override void SetCommands()
        {
            base.SetCommands();
            SaveCommand = new MvxAsyncCommand(Save);
            CloseCommand = new MvxAsyncCommand(
                async () => await _navigationService.CloseSettingResult(this, NavigationBoolResult.Fail()));
        }

        private async Task Save()
        {
            if (!IsValid)
                return;

            IsBusy = true;
            try
            {
                if (_id.HasValue)
                    await _castItHub.UpdateCustomEndpoint(_id.Value, Name, Host, Port);
                else
                    await _castItHub.AddCustomEndpoint(Name, Host, Port);

                await _navigationService.CloseSettingResult(this, NavigationBoolResult.Succeed());
            }
            catch (System.Exception e)
            {
                Logger.LogError(e, $"{nameof(Save)}: Unknown error");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
