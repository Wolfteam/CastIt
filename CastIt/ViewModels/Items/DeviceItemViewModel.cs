using CastIt.Interfaces;
using Microsoft.Extensions.Logging;
using MvvmCross.Plugin.Messenger;

namespace CastIt.ViewModels.Items
{
    public class DeviceItemViewModel : BaseViewModel
    {
        private bool _isSelected;

        public string Id { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string FriendlyName { get; set; }
        public string IpAdress
            => $"{Host}:{Port}";

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
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
