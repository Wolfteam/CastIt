using CastIt.Interfaces;
using MvvmCross.Logging;
using MvvmCross.Plugin.Messenger;

namespace CastIt.ViewModels.Items
{
    public class DeviceItemViewModel : BaseViewModel
    {
        private string _name;
        private string _type;
        private bool _isSelected;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public DeviceItemViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            IMvxLogProvider logger)
            : base(textProvider, messenger, logger.GetLogFor<DeviceItemViewModel>())
        {
        }
    }
}
