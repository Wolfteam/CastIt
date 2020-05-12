using CastIt.Interfaces;
using CastIt.Models;
using CastIt.ViewModels.Items;
using MvvmCross;
using MvvmCross.Commands;
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

        public IMvxCommand<DeviceItemViewModel> ConnectCommand { get; private set; }
        public IMvxCommand<DeviceItemViewModel> DisconnectCommand { get; private set; }

        public DevicesViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            IMvxLogProvider logger,
            ICastService castService)
            : base(textProvider, messenger, logger.GetLogFor<DevicesViewModel>())
        {
            _castService = castService;
            var devices = _castService.AvailableDevices.Select(d =>
            {
                var vm = Mvx.IoCProvider.IoCConstruct<DeviceItemViewModel>();
                vm.Name = d.Name;
                vm.Type = d.Type;

                return vm;
            }).ToList();

            Devices.AddRange(devices);

            _castService.OnCastRendererSet += OnCastRendererSet;
            _castService.OnCastableDeviceAdded += OnCastDeviceAdded;
            _castService.OnCastableDeviceDeleted += OnCastDeviceDeleted;
        }

        public override void SetCommands()
        {
            base.SetCommands();
            ConnectCommand = new MvxCommand<DeviceItemViewModel>(
                (device) => ToggleConectedDevice(device));

            DisconnectCommand = new MvxCommand<DeviceItemViewModel>(
                (_) => ToggleConectedDevice(null));
        }

        public void CleanUp()
        {
            _castService.OnCastableDeviceAdded -= OnCastDeviceAdded;
            _castService.OnCastableDeviceDeleted -= OnCastDeviceDeleted;
        }

        private void ToggleConectedDevice(DeviceItemViewModel device)
        {
            foreach (var item in Devices)
            {
                item.IsSelected = false;
            }
            _castService.SetCastRenderer(device?.Name, device?.Type);
        }

        private void OnCastRendererSet(string name, string type)
        {
            var renderer = Devices.FirstOrDefault(d => d.Name == name && d.Type == type);
            if (renderer == null)
                return;

            foreach (var item in Devices)
            {
                item.IsSelected = false;
            }
            renderer.IsSelected = true;
        }

        private void OnCastDeviceAdded(CastableDevice device)
        {
            if (Devices.Any(d => d.Name == device.Name && d.Type == device.Type))
                return;

            var vm = Mvx.IoCProvider.IoCConstruct<DeviceItemViewModel>();
            vm.Name = device.Name;
            vm.Type = device.Type;

            Devices.Add(vm);
        }

        private void OnCastDeviceDeleted(CastableDevice device)
        {
            var toDelete = Devices.FirstOrDefault(d => d.Name == device.Name && d.Type == device.Type);
            if (toDelete == null)
                return;
            Devices.Remove(toDelete);
        }
    }
}
