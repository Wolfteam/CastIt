using AutoMapper;
using CastIt.GoogleCast.Interfaces;
using CastIt.Interfaces;
using CastIt.Models.Messages;
using CastIt.ViewModels.Items;
using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CastIt.ViewModels
{
    public class DevicesViewModel : BaseViewModel
    {
        private readonly ICastService _castService;
        private readonly IMapper _mapper;
        public MvxObservableCollection<DeviceItemViewModel> Devices { get; set; }
            = new MvxObservableCollection<DeviceItemViewModel>();

        public IMvxAsyncCommand<DeviceItemViewModel> ConnectCommand { get; private set; }
        public IMvxAsyncCommand<DeviceItemViewModel> DisconnectCommand { get; private set; }

        public DevicesViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            IMvxLogProvider logger,
            ICastService castService,
            IMapper mapper)
            : base(textProvider, messenger, logger.GetLogFor<DevicesViewModel>())
        {
            _castService = castService;
            _mapper = mapper;

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
            ConnectCommand = new MvxAsyncCommand<DeviceItemViewModel>(ToggleConectedDevice);

            DisconnectCommand = new MvxAsyncCommand<DeviceItemViewModel>(
                (_) => ToggleConectedDevice(null));
        }

        public void CleanUp()
        {
            _castService.OnCastableDeviceAdded -= OnCastDeviceAdded;
            _castService.OnCastableDeviceDeleted -= OnCastDeviceDeleted;
            _castService.OnDisconnected -= DeSelectAllDevices;
        }

        private Task ToggleConectedDevice(DeviceItemViewModel device)
        {
            DeSelectAllDevices();

            Messenger.Publish(new ManualDisconnectMessage(this));
            return _castService.SetCastRenderer(device?.Id);
        }

        private void OnCastRendererSet(string id)
        {
            var renderer = Devices.FirstOrDefault(d => d.Id == id);
            if (renderer == null)
                return;

            DeSelectAllDevices();
            renderer.IsSelected = true;
        }

        private void OnCastDeviceAdded(IReceiver receiver)
        {
            if (Devices.Any(d => d.Id == receiver.Id))
                return;

            var vm = _mapper.Map<DeviceItemViewModel>(receiver);
            Devices.Add(vm);
        }

        private void OnCastDeviceDeleted(IReceiver receiver)
        {
            var toDelete = Devices.FirstOrDefault(d => d.Id == receiver.Id);
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
    }
}
