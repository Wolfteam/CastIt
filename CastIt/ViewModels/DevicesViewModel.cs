using CastIt.Interfaces;
using CastIt.Models;
using CastIt.ViewModels.Items;
using MvvmCross.Logging;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using System.Linq;

namespace CastIt.ViewModels
{
    public class DevicesViewModel : BaseViewModel
    {
        private readonly ICastService _castService;
        public MvxObservableCollection<DeviceItemViewModel> Devices { get; set; }
            = new MvxObservableCollection<DeviceItemViewModel>();
        public DevicesViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            IMvxLogProvider logger,
            ICastService castService)
            : base(textProvider, messenger, logger.GetLogFor<DevicesViewModel>())
        {
            _castService = castService;
            var devices = _castService.AvailableDevices.Select(d => new DeviceItemViewModel
            {
                Name = d.Name,
                Type = d.Type
            }).ToList();

            Devices.AddRange(devices);
            _castService.OnCastableDeviceAdded += OnCastDeviceAdded;
        }

        public void CleanUp()
        {
            _castService.OnCastableDeviceAdded -= OnCastDeviceAdded;
        }

        private void OnCastDeviceAdded(CastableDevice device)
        {
            if (Devices.Any(d => d.Name == device.Name && d.Type == device.Type))
                return;

            Devices.Add(new DeviceItemViewModel
            {
                Name = device.Name,
                Type = device.Type
            });
        }
    }
}
