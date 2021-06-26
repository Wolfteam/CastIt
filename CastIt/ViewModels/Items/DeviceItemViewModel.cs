using CastIt.Interfaces;
using Microsoft.Extensions.Logging;
using MvvmCross.Plugin.Messenger;

namespace CastIt.ViewModels.Items
{
    public class DeviceItemViewModel : BaseViewModel
    {
        private bool _isConnected;

        public string Id { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string FriendlyName { get; set; }
        public string IpAddress
            => $"{Host}:{Port}";

        public bool IsConnected
        {
            get => _isConnected;
            set => SetProperty(ref _isConnected, value);
        }

        public DeviceItemViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger<DeviceItemViewModel> logger)
            : base(textProvider, messenger, logger)
        {
        }
    }
}
