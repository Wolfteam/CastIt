using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Enums;
using CastIt.Domain.Interfaces;
using CastIt.Infrastructure.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CastIt.Server.Interfaces
{
    //The exposed name methods here must match the ones that the client listens for
    public interface ICastItHub
    {
        Task SendPlayLists(List<GetAllPlayListResponseDto> playLists);

        Task StoppedPlayBack();

        Task PlayListAdded(GetAllPlayListResponseDto playList);

        Task PlayListChanged(GetAllPlayListResponseDto playList);

        Task PlayListsChanged(List<GetAllPlayListResponseDto> playLists);

        Task PlayListDeleted(long id);

        Task PlayListIsBusy(long id, bool isBusy);

        Task FileAdded(FileItemResponseDto file);

        Task FileChanged(FileItemResponseDto file);

        Task FilesChanged(List<FileItemResponseDto> files);

        Task FileDeleted(long playListId, long id);

        Task FileLoading(FileItemResponseDto file);

        Task FileLoaded(FileItemResponseDto file);

        Task FileEndReached(FileItemResponseDto file);

        //Task FileStoppedPlayback(FileItemResponseDto file);

        //Task CurrentPlayedFileStatusChanged(ServerFileItem file);

        Task PlayerStatusChanged(ServerPlayerStatusResponseDto status);

        Task PlayerSettingsChanged(ServerAppSettings settings);

        Task ServerMessage(AppMessageType type);

        Task CastDeviceSet(IReceiver device);

        Task CastDevicesChanged(List<IReceiver> devices);

        Task CastDeviceDisconnected();
    }
}
