using CastIt.GoogleCast.Shared.Device;
using CastIt.Interfaces;
using CastIt.Models.Messages;
using CastIt.ViewModels.Items;
using Microsoft.Extensions.Logging;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CastIt.ViewModels
{
    public class DevicesViewModel : BasePopupViewModel
    {
        private readonly ICastItHubClientService _castItHub;

        private bool _isConnecting;
        private bool _isRefreshing;

        public bool IsConnecting
        {
            get => _isConnecting;
            set => SetProperty(ref _isConnecting, value);
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        public MvxObservableCollection<DeviceItemViewModel> Devices { get; set; }
            = new MvxObservableCollection<DeviceItemViewModel>();

        public IMvxAsyncCommand<DeviceItemViewModel> ConnectCommand { get; private set; }
        public IMvxAsyncCommand<DeviceItemViewModel> DisconnectCommand { get; private set; }
        public IMvxAsyncCommand RefreshDevicesCommand { get; private set; }

        public DevicesViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger<DevicesViewModel> logger,
            ICastItHubClientService castItHub)
            : base(textProvider, messenger, logger)
        {
            _castItHub = castItHub;
            _castItHub.OnCastDeviceSet += OnCastDeviceSet;
            _castItHub.OnCastDevicesChanged += OnCastDevicesChanged;
            _castItHub.OnCastDeviceDisconnected += DeSelectAllDevices;
            _castItHub.OnClientDisconnected += CastItHubOnOnClientDisconnected;
        }

        public override void SetCommands()
        {
            base.SetCommands();
            ConnectCommand = new MvxAsyncCommand<DeviceItemViewModel>(ToggleConnectedDevice);

            DisconnectCommand = new MvxAsyncCommand<DeviceItemViewModel>(
                (_) => ToggleConnectedDevice(null));

            RefreshDevicesCommand = new MvxAsyncCommand(RefreshDevices);
        }

        public void CleanUp()
        {
            _castItHub.OnClientDisconnected -= CastItHubOnOnClientDisconnected;
            _castItHub.OnCastDeviceSet -= OnCastDeviceSet;
            _castItHub.OnCastDevicesChanged -= OnCastDevicesChanged;
            _castItHub.OnCastDeviceDisconnected -= DeSelectAllDevices;
            _castItHub.OnClientDisconnected -= CastItHubOnOnClientDisconnected;
        }

        private async Task ToggleConnectedDevice(DeviceItemViewModel device)
        {
            IsConnecting = true;

            DeSelectAllDevices();
            Messenger.Publish(new ManualDisconnectMessage(this));
            try
            {
                await _castItHub.ConnectToCastDevice(device?.Id);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Couldn't connect to = {device?.FriendlyName}, maybe the device was off ?");
                Messenger.Publish(new SnackbarMessage(this, GetText("DeviceConnectionFailed")));
            }
            IsConnecting = false;
        }

        private void OnCastDeviceSet(IReceiver device)
        {
            var renderer = Devices.FirstOrDefault(d => d.Id == device.Id);
            if (renderer == null)
                return;

            DeSelectAllDevices();
            renderer.IsConnected = device.IsConnected;
        }

        private void OnCastDevicesChanged(List<IReceiver> devices)
        {
            Devices.Clear();
            foreach (var device in devices)
            {
                OnCastDeviceAdded(device);
            }
        }

        private void OnCastDeviceAdded(IReceiver device)
        {
            if (Devices.Any(d => d.Id == device.Id))
            {
                var existing = Devices.First(d => d.Id == device.Id);
                existing.FriendlyName = device.FriendlyName;
                existing.Host = device.Host;
                existing.Port = device.Port;
                existing.IsConnected = device.IsConnected;
                return;
            }

            var vm = Mvx.IoCProvider.Resolve<DeviceItemViewModel>();
            vm.Id = device.Id;
            vm.FriendlyName = device.FriendlyName;
            vm.Host = device.Host;
            vm.Port = device.Port;
            vm.IsConnected = device.IsConnected;
            Devices.Add(vm);
        }

        private void OnCastDeviceDeleted(IReceiver device)
        {
            var toDelete = Devices.FirstOrDefault(d => d.Id == device.Id);
            if (toDelete == null)
                return;
            Devices.Remove(toDelete);
        }

        private void DeSelectAllDevices()
        {
            foreach (var device in Devices)
            {
                device.IsConnected = false;
            }
        }

        private async Task RefreshDevices()
        {
            var selected = Devices.FirstOrDefault(d => d.IsConnected);
            Logger.LogInformation($"{nameof(RefreshDevices)}: Refreshing list of devices, currently we got = {Devices.Count} device(s) and the selected one is = {selected?.FriendlyName}...");

            IsRefreshing = true;
            Devices.Clear();
            //The refresh will take care of populating the devices collection
            await _castItHub.RefreshCastDevices(TimeSpan.FromSeconds(5));
            IsRefreshing = false;
        }

        private void CastItHubOnOnClientDisconnected()
        {
            Devices.Clear();
        }
    }
}
