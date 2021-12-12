using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Enums;
using CastIt.GoogleCast.Models.Events;
using CastIt.GoogleCast.Shared.Device;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CastIt.Server.Services
{
    public partial class ServerCastService
    {
        #region Events handlers
        public void FileLoading(object sender, EventArgs e)
            => FileLoading();

        private async void EndReached(object sender, EventArgs e)
        {
            SendEndReached();
            await GoTo(true, true);
        }

        private void PositionChanged(object sender, double position)
            => SendPositionChanged(position);

        private void TimeChanged(object sender, double seconds)
            => SendTimeChanged(seconds);

        private void Paused(object sender, EventArgs e)
            => SendPaused();

        private void Disconnected(object sender, EventArgs e)
            => SendDisconnected();

        private void VolumeLevelChanged(object sender, double e)
            => SendVolumeLevelChanged(e);

        private void IsMutedChanged(object sender, bool isMuted)
            => SendIsMutedChanged(isMuted);

        private void RendererDiscovererItemAdded(object sender, DeviceAddedArgs e)
            => SendRendererDiscovererItemAdded(e.Receiver);

        private void LoadFailed(object sender, EventArgs e)
            => SendErrorLoadingFile();
        #endregion

        #region Event Senders

        public void SendEndReached()
            => _server.OnEndReached?.Invoke();

        public void SendPositionChanged(double position)
        {
            if (position > 100)
            {
                position = 100;
            }
            _server.OnPositionChanged?.Invoke(position);
        }

        public void SendTimeChanged(double seconds)
            => _server.OnTimeChanged?.Invoke(seconds);

        public void SendPaused()
            => _server.OnPaused?.Invoke();

        public void SendDevicesChanged()
            => _server.OnCastDevicesChanged?.Invoke(AvailableDevices);

        public void SendDisconnected()
        {
            _renderWasSet = false;
            foreach (var device in AvailableDevices)
            {
                device.IsConnected = false;
            }
            SendDevicesChanged();
            _server.OnDisconnected?.Invoke();
        }

        public void SendVolumeLevelChanged(double newValue)
            => _server.OnVolumeChanged?.Invoke(newValue, _player.IsMuted);

        public void SendIsMutedChanged(bool isMuted)
            => _server.OnVolumeChanged?.Invoke(_player.CurrentVolumeLevel, isMuted);

        public void SendRendererDiscovererItemAdded(IReceiver device)
        {
            _logger.LogInformation(
                $"{nameof(RendererDiscovererItemAdded)}: New device discovered: " +
                $"{device.FriendlyName} - Ip = {device.Host}:{device.Port}");
            if (AvailableDevices.All(d => d.Id != device.Id))
            {
                AvailableDevices.Add(device);
            }
            _server.OnCastableDeviceAdded?.Invoke(device);
        }

        public void SendErrorLoadingFile()
            => _server.OnServerMessage?.Invoke(AppMessageType.UnknownErrorLoadingFile);

        public void SendNoDevicesFound()
            => _server.OnServerMessage?.Invoke(AppMessageType.NoDevicesFound);

        public void SendNoInternetConnection()
            => _server.OnServerMessage?.Invoke(AppMessageType.NoInternetConnection);

        public void SendPlayListNotFound()
            => _server.OnServerMessage?.Invoke(AppMessageType.PlayListNotFound);

        public void SendFileNotFound()
            => _server.OnServerMessage?.Invoke(AppMessageType.FileNotFound);

        public void SendInvalidRequest()
            => _server.OnServerMessage?.Invoke(AppMessageType.InvalidRequest);

        public void SendServerIsClosing()
            => _server.OnServerMessage?.Invoke(AppMessageType.ServerIsClosing);

        public void SendPlayListAdded(GetAllPlayListResponseDto playList)
            => _server.OnPlayListAdded?.Invoke(playList);

        public void SendPlayListChanged(GetAllPlayListResponseDto playList)
            => _server.OnPlayListChanged?.Invoke(playList);

        public void SendPlayListsChanged(List<GetAllPlayListResponseDto> playLists)
            => _server.OnPlayListsChanged?.Invoke(playLists);

        public void SendPlayListBusy(long id, bool isBusy)
            => _server.OnPlayListBusy?.Invoke(id, isBusy);

        public void SendPlayListDeleted(long id)
            => _server.OnPlayListDeleted?.Invoke(id);

        public void SendFileAdded(FileItemResponseDto file)
            => _server.OnFileAdded?.Invoke(file);

        public void SendFileChanged(FileItemResponseDto file)
            => _server.OnFileChanged?.Invoke(file);

        public void SendFilesChanged(List<FileItemResponseDto> files)
            => _server.OnFilesChanged?.Invoke(files);

        public void SendFileDeleted(long playListId, long id)
            => _server.OnFileDeleted?.Invoke(playListId, id);

        public void SendServerMsg(AppMessageType type)
            => _server.OnServerMessage?.Invoke(type);
        #endregion

        //TODO: CHECK IF WE CAN KNOW WHEN A DEVICE IS REMOVED
        //private void RendererDiscovererItemDeleted(object sender, RendererDiscovererItemDeletedEventArgs e)
        //{
        //    _logger.LogInformation(
        //        $"{nameof(RendererDiscovererItemAdded)}: Item removed: " +
        //        $"{e.RendererItem.Name} of type {e.RendererItem.Type}");
        //    _rendererItems.Remove(e.RendererItem);
        //    OnCastableDeviceDeleted?.Invoke(new CastableDevice
        //    {
        //        Name = e.RendererItem.Name,
        //        Type = e.RendererItem.Type
        //    });
        //}
    }
}
