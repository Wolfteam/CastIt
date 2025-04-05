using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Enums;
using CastIt.GoogleCast.Shared.Device;
using CastIt.Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CastIt.Interfaces
{
    public interface ICastItHubClientService
    {
        event Action OnClientConnected;
        event Action OnClientDisconnected;

        event Action<ServerPlayerStatusResponseDto> OnPlayerStatusChanged;
        event Action<ServerAppSettings> OnPlayerSettingsChanged;
        event Action<IReceiver> OnCastDeviceSet;
        event Action<List<IReceiver>> OnCastDevicesChanged;
        event Action OnCastDeviceDisconnected;
        event Action<AppMessageType> OnServerMessage;
        event Action OnStoppedPlayback;

        event Action<List<GetAllPlayListResponseDto>> OnPlayListsLoaded;
        event Action<GetAllPlayListResponseDto> OnPlayListAdded;
        event Action<GetAllPlayListResponseDto> OnPlayListChanged;
        event Action<List<GetAllPlayListResponseDto>> OnPlayListsChanged;
        event Action<long> OnPlayListDeleted;
        event Action<long, bool> OnPlayListBusy;

        event Action<FileItemResponseDto> OnFileAdded;
        event Action<FileItemResponseDto> OnFileChanged;
        event Action<List<FileItemResponseDto>> OnFilesChanged;
        event Action<long, long> OnFileDeleted;
        event Action<FileItemResponseDto> OnFileLoading;
        event Action<FileItemResponseDto> OnFileLoaded;
        event Action<FileItemResponseDto> OnFileEndReached;

        Task<bool> Init(string serverIpAddress);
        Task DisposeAsync();

        Task Play(long playListId, long fileId, bool force, bool fileOptionsChanged = false);
        Task GoToSeconds(double seconds);
        Task SkipSeconds(double seconds);
        Task GoTo(bool next, bool previous);
        Task TogglePlayBack();
        Task StopPlayBack();
        Task SetVolume(double level, bool isMuted);
        Task UpdateSettings(ServerAppSettings settings);
        Task ConnectToCastDevice(string id);
        Task RefreshCastDevices(TimeSpan ts);
        Task SetFileSubtitlesFromPath(string path);

        Task<PlayListItemResponseDto> AddNewPlayList();
        Task<PlayListItemResponseDto> GetPlayList(long id);
        Task UpdatePlayList(long id, string name);
        Task UpdatePlayListPosition(long playListId, int newIndex);
        Task SetPlayListOptions(long playListId, bool loop, bool shuffle);
        Task DeletePlayList(long id);
        Task DeleteAllPlayLists(long exceptId);
        Task RemoveFilesThatStartsWith(long playListId, string path);
        Task RemoveFiles(long playListId, List<long> ids);
        Task RemoveAllMissingFiles(long playListId);
        Task AddFolders(long playListId, bool includeSubFolder, params string[] folders);
        Task AddFiles(long playListId, params string[] files);
        Task AddUrlFile(long playListId, string url, bool onlyVideo);
        Task SortFiles(long playListId, SortModeType sortMode);

        Task LoopFile(long playListId, long id, bool loop);
        Task SetFileOptions(int streamIndex, bool isAudio, bool isSubTitle, bool isQuality);
        Task DeleteFile(long playListId, long id);
        Task UpdateFilePosition(long playListId, long id, int newIndex);
    }
}
