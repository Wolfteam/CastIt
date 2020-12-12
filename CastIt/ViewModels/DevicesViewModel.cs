using AutoMapper;
using CastIt.GoogleCast.Interfaces;
using CastIt.Infrastructure.Interfaces;
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
        private readonly ICastService _castService;
        private readonly IMapper _mapper;
        private readonly IPlayer _player;

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
            ICastService castService,
            IMapper mapper,
            IPlayer player)
            : base(textProvider, messenger, logger)
        {
            _castService = castService;
            _mapper = mapper;
            _player = player;

            var devices = _mapper.Map<List<DeviceItemViewModel>>(_castService.AvailableDevices);

            Devices.AddRange(devices);

            _castService.OnCastRendererSet += OnCastRendererSet;
            _castService.OnCastableDeviceAdded += OnCastDeviceAdded;
            _castService.OnCastableDeviceDeleted += OnCastDeviceDeleted;
            _castService.OnDisconnected += DeSelectAllDevices;
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
            _castService.OnCastableDeviceAdded -= OnCastDeviceAdded;
            _castService.OnCastableDeviceDeleted -= OnCastDeviceDeleted;
            _castService.OnDisconnected -= DeSelectAllDevices;
        }

        private async Task ToggleConnectedDevice(DeviceItemViewModel device)
        {
            IsConnecting = true;

            DeSelectAllDevices();
            Messenger.Publish(new ManualDisconnectMessage(this));
            try
            {
                await _castService.SetCastRenderer(device?.Id);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Couldn't connect to = {device?.FriendlyName}, maybe the device was off ?");
                Messenger.Publish(new SnackbarMessage(this, GetText("DeviceConnectionFailed")));
            }
            IsConnecting = false;
        }

        private void OnCastRendererSet(string id)
        {
            var renderer = Devices.FirstOrDefault(d => d.Id == id);
            if (renderer == null)
                return;

            DeSelectAllDevices();
            renderer.IsSelected = true;
        }

        private void OnCastDeviceAdded(string id, string friendlyName, string type, string host, int port)
        {
            if (Devices.Any(d => d.Id == id))
                return;

            var vm = Mvx.IoCProvider.Resolve<DeviceItemViewModel>();
            vm.Id = id;
            vm.FriendlyName = friendlyName;
            vm.Host = vm.Host;
            vm.Port = port;
            Devices.Add(vm);
        }

        private void OnCastDeviceDeleted(string id)
        {
            var toDelete = Devices.FirstOrDefault(d => d.Id == id);
            if (toDelete == null)
                return;
            Devices.Remove(toDelete);
        }

        private void DeSelectAllDevices()
        {
            foreach (var device in Devices)
            {
                device.IsSelected = false;
            }
        }

        private async Task RefreshDevices()
        {
            var selected = Devices.FirstOrDefault(d => d.IsSelected);
            Logger.LogInformation($"{nameof(RefreshDevices)}: Refreshing list of devices, currently we got = {Devices.Count} device(s) and the selected one is = {selected?.FriendlyName}...");
            IsRefreshing = true;

            _castService.AvailableDevices.Clear();
            Devices.Clear();
            var devices = await _player.GetDevicesAsync(TimeSpan.FromSeconds(5));
            devices = devices.OrderBy(d => d.FriendlyName).ToList();
            Devices.SwitchTo(_mapper.Map<List<DeviceItemViewModel>>(devices));
            _castService.AvailableDevices.AddRange(devices);

            if (selected != null)
            {
                var refreshed = Devices.FirstOrDefault(d => d.Id == selected.Id);
                if (refreshed != null)
                    refreshed.IsSelected = true;
            }
            Logger.LogInformation($"{nameof(RefreshDevices)}: Refresh completed, got = {Devices.Count} device(s)");

            IsRefreshing = false;
        }
    }
}
