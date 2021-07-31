using CastIt.Application.Server;
using CastIt.Domain.Dtos.Requests;
using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Enums;
using CastIt.Domain.Interfaces;
using CastIt.Domain.Models.Device;
using CastIt.Infrastructure.Models;
using CastIt.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CastIt.Services
{
    public class CastItHubClientService : ICastItHubClientService
    {
        private readonly ILogger<CastItHubClientService> _logger;
        private HubConnection _connection;

        public event Action OnClientConnected;
        public event Action OnClientDisconnected;

        public event Action<ServerPlayerStatusResponseDto> OnPlayerStatusChanged;
        public event Action<ServerAppSettings> OnPlayerSettingsChanged;
        public event Action<IReceiver> OnCastDeviceSet;
        public event Action<List<IReceiver>> OnCastDevicesChanged;
        public event Action OnCastDeviceDisconnected;
        public event Action<AppMessageType> OnServerMessage;
        public event Action OnStoppedPlayback;

        public event Action<List<GetAllPlayListResponseDto>> OnPlayListsLoaded;
        public event Action<GetAllPlayListResponseDto> OnPlayListAdded;
        public event Action<GetAllPlayListResponseDto> OnPlayListChanged;
        public event Action<List<GetAllPlayListResponseDto>> OnPlayListsChanged;
        public event Action<long> OnPlayListDeleted;
        public event Action<long, bool> OnPlayListBusy;

        public event Action<FileItemResponseDto> OnFileAdded;
        public event Action<FileItemResponseDto> OnFileChanged;
        public event Action<List<FileItemResponseDto>> OnFilesChanged;
        public event Action<long, long> OnFileDeleted;
        public event Action<FileItemResponseDto> OnFileLoading;
        public event Action<FileItemResponseDto> OnFileLoaded;
        public event Action<FileItemResponseDto> OnFileEndReached;

        public string IpAddress { get; set; }

        public CastItHubClientService(ILogger<CastItHubClientService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> Init()
        {
            //Kinda hack...
            bool serverStarted = await StartServerIfNotStarted();
            if (!serverStarted)
                return false;

            bool connectionBuilt = await BuildConnection();
            if (!connectionBuilt)
                return false;

            ListenForEvents();
            var connected = await ConnectToHub();
            if (connected)
                OnClientConnected?.Invoke();

            return connected;
        }

        public async Task<bool> Init(string serverIpAddress)
        {
            if (string.IsNullOrWhiteSpace(serverIpAddress) || !Uri.TryCreate(serverIpAddress, UriKind.Absolute, out _))
            {
                throw new ArgumentOutOfRangeException(nameof(serverIpAddress), serverIpAddress, "You need to provide a valid server ip address");
            }

            bool connectionBuilt = await BuildConnection(serverIpAddress, 1);
            if (!connectionBuilt)
                return false;

            ListenForEvents();
            var connected = await ConnectToHub(1);
            if (connected)
                OnClientConnected?.Invoke();

            return connected;
        }

        public async Task DisposeAsync()
        {
            OnClientDisconnected = null;
            OnClientConnected = null;
            OnPlayerStatusChanged = null;
            OnPlayerSettingsChanged = null;
            OnCastDeviceSet = null;
            OnCastDevicesChanged = null;
            OnServerMessage = null;
            OnStoppedPlayback = null;
            OnPlayListsLoaded = null;
            OnPlayListAdded = null;
            OnPlayListChanged = null;
            OnPlayListsChanged = null;
            OnPlayListDeleted = null;
            OnPlayListBusy = null;
            OnFileAdded = null;
            OnFileChanged = null;
            OnFilesChanged = null;
            OnFileDeleted = null;
            OnFileLoading = null;
            OnFileLoaded = null;
            OnFileEndReached = null;
            if (_connection != null)
            {
                await _connection.DisposeAsync();
            }
        }

        #region Player methods
        public async Task Play(long playListId, long fileId, bool force, bool fileOptionsChanged = false)
        {
            try
            {
                var request = new PlayFileRequestDto
                {
                    Id = fileId,
                    PlayListId = playListId,
                    Force = force,
                    FileOptionsChanged = fileOptionsChanged
                };
                await _connection.InvokeAsync(nameof(Play), request);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(Play)}: Unknown error occurred");
            }
        }

        public async Task GoToSeconds(double seconds)
        {
            try
            {
                await _connection.InvokeAsync(nameof(GoToSeconds), seconds);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(GoToSeconds)}: Unknown error occurred");
            }
        }

        public async Task SkipSeconds(double seconds)
        {
            try
            {
                await _connection.InvokeAsync(nameof(SkipSeconds), seconds);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(SkipSeconds)}: Unknown error occurred");
            }
        }

        public async Task GoTo(bool next, bool previous)
        {
            try
            {
                await _connection.InvokeAsync(nameof(GoTo), next, previous);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(GoTo)}: Unknown error occurred");
            }
        }

        public async Task TogglePlayBack()
        {
            try
            {
                await _connection.InvokeAsync(nameof(TogglePlayBack));
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(TogglePlayBack)}: Unknown error occurred");
            }
        }

        public async Task StopPlayBack()
        {
            try
            {
                await _connection.InvokeAsync(nameof(StopPlayBack));
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(StopPlayBack)}: Unknown error occurred");
            }
        }

        public async Task SetVolume(double level, bool isMuted)
        {
            var request = new SetVolumeRequestDto
            {
                IsMuted = isMuted,
                VolumeLevel = level
            };
            try
            {
                await _connection.InvokeAsync(nameof(SetVolume), request);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(SetVolume)}: Unknown error occurred");
            }
        }

        public async Task UpdateSettings(ServerAppSettings settings)
        {
            try
            {
                await _connection.InvokeAsync(nameof(UpdateSettings), settings);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(UpdateSettings)}: Unknown error occurred");
            }
        }

        public async Task ConnectToCastDevice(string id)
        {
            try
            {
                await _connection.InvokeAsync(nameof(ConnectToCastDevice), id);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(ConnectToCastDevice)}: Unknown error occurred");
            }
        }

        public async Task RefreshCastDevices(TimeSpan ts)
        {
            try
            {
                await _connection.InvokeAsync(nameof(RefreshCastDevices), ts);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(RefreshCastDevices)}: Unknown error occurred");
            }
        }

        public async Task SetFileSubtitlesFromPath(string path)
        {
            try
            {
                await _connection.InvokeAsync(nameof(SetFileSubtitlesFromPath), path);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(SetFileSubtitlesFromPath)}: Unknown error occurred");
            }
        }
        #endregion

        #region PlayList Methods

        public async Task<PlayListItemResponseDto> AddNewPlayList()
        {
            try
            {
                return await _connection.InvokeAsync<PlayListItemResponseDto>(nameof(AddNewPlayList));
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(AddNewPlayList)}: Unknown error occurred");
                throw;
            }
        }

        public async Task<PlayListItemResponseDto> GetPlayList(long id)
        {
            try
            {
                return await _connection.InvokeAsync<PlayListItemResponseDto>(nameof(GetPlayList), id);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(GetPlayList)}: Unknown error occurred");
                throw;
            }
        }

        public async Task UpdatePlayList(long id, string name)
        {
            var request = new UpdatePlayListRequestDto
            {
                Name = name,
            };
            try
            {
                await _connection.InvokeAsync(nameof(UpdatePlayList), id, request);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(UpdatePlayList)}: Unknown error occurred");
            }
        }

        public async Task UpdatePlayListPosition(long playListId, int newIndex)
        {
            try
            {
                await _connection.InvokeAsync(nameof(UpdatePlayListPosition), playListId, newIndex);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(UpdatePlayListPosition)}: Unknown error occurred");
            }
        }

        public async Task SetPlayListOptions(long playListId, bool loop, bool shuffle)
        {
            var request = new SetPlayListOptionsRequestDto
            {
                Loop = loop,
                Shuffle = shuffle
            };
            try
            {
                await _connection.InvokeAsync(nameof(SetPlayListOptions), playListId, request);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(SetPlayListOptions)}: Unknown error occurred");
            }
        }

        public async Task DeletePlayList(long id)
        {
            try
            {
                await _connection.InvokeAsync(nameof(DeletePlayList), id);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(DeletePlayList)}: Unknown error occurred");
            }
        }

        public async Task DeleteAllPlayLists(long exceptId)
        {
            try
            {
                await _connection.InvokeAsync(nameof(DeleteAllPlayLists), exceptId);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(DeleteAllPlayLists)}: Unknown error occurred");
            }
        }

        public async Task RemoveFiles(long playListId, List<long> ids)
        {
            try
            {
                await _connection.InvokeAsync(nameof(RemoveFiles), playListId, ids);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(RemoveFiles)}: Unknown error occurred");
            }
        }

        public async Task RemoveFilesThatStartsWith(long playListId, string path)
        {
            try
            {
                await _connection.InvokeAsync(nameof(RemoveFilesThatStartsWith), playListId, path);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(RemoveFilesThatStartsWith)}: Unknown error occurred");
            }
        }

        public async Task RemoveAllMissingFiles(long playListId)
        {
            try
            {
                await _connection.InvokeAsync(nameof(RemoveAllMissingFiles), playListId);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(RemoveAllMissingFiles)}: Unknown error occurred");
            }
        }

        public async Task AddFolders(long playListId, bool includeSubFolder, params string[] folders)
        {
            var request = new AddFolderOrFilesToPlayListRequestDto
            {
                Folders = folders.ToList(),
                IncludeSubFolders = includeSubFolder
            };

            try
            {
                await _connection.InvokeAsync(nameof(AddFolders), playListId, request);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(AddFolders)}: Unknown error occurred");
            }
        }

        public async Task AddFiles(long playListId, params string[] files)
        {
            var request = new AddFolderOrFilesToPlayListRequestDto
            {
                Files = files.ToList(),
            };

            try
            {
                await _connection.InvokeAsync(nameof(AddFiles), playListId, request);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(AddFiles)}: Unknown error occurred");
            }
        }

        public async Task AddUrlFile(long playListId, string url, bool onlyVideo)
        {
            var request = new AddUrlToPlayListRequestDto
            {
                Url = url,
                OnlyVideo = onlyVideo
            };

            try
            {
                await _connection.InvokeAsync(nameof(AddUrlFile), playListId, request);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(AddUrlFile)}: Unknown error occurred");
            }
        }

        public async Task SortFiles(long playListId, SortModeType sortMode)
        {
            try
            {
                await _connection.InvokeAsync(nameof(SortFiles), playListId, sortMode);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(SortFiles)}: Unknown error occurred");
            }
        }
        #endregion

        #region File Methods
        public async Task LoopFile(long playListId, long id, bool loop)
        {
            try
            {
                await _connection.InvokeAsync(nameof(LoopFile), playListId, id, loop);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(LoopFile)}: Unknown error occurred");
            }
        }

        public async Task DeleteFile(long playListId, long id)
        {
            try
            {
                await _connection.InvokeAsync(nameof(DeleteFile), playListId, id);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(DeleteFile)}: Unknown error occurred");
            }
        }

        public async Task SetFileOptions(int streamIndex, bool isAudio, bool isSubTitle, bool isQuality)
        {
            var request = new SetFileOptionsRequestDto
            {
                StreamIndex = streamIndex,
                IsAudio = isAudio,
                IsSubTitle = isSubTitle,
                IsQuality = isQuality,
            };
            try
            {
                await _connection.InvokeAsync(nameof(SetFileOptions), request);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(SetFileOptions)}: Unknown error occurred");
            }
        }

        public async Task UpdateFilePosition(long playListId, long id, int newIndex)
        {
            try
            {
                await _connection.InvokeAsync(nameof(UpdateFilePosition), playListId, id, newIndex);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(UpdateFilePosition)}: Unknown error occurred");
            }
        }
        #endregion

        #region Helpers
        //TODO: ONCE THIS GETS IMPLEMENTED, REPLACE THIS CODE
        //https://github.com/dotnet/aspnetcore/issues/32534
        public void ListenForEvents()
        {
            //Remember that all the T on On<T> has to be a class with an empty constructor
            ListenPlayerEvents();

            ListenPlayListsEvents();

            ListenFileEvents();
        }

        public async Task<bool> StartServerIfNotStarted()
        {
            int retries = 3;
            while (retries > 0)
            {
                if (WebServerUtils.IsServerAlive())
                    return true;

                _logger.LogInformation($"{nameof(StartServerIfNotStarted)}: Server has not been started... Starting it with retries left = {retries}...");
                bool started = WebServerUtils.StartServer();
                if (started)
                    return true;
                _logger.LogWarning($"{nameof(StartServerIfNotStarted)}: Server could not be started");
                await Task.Delay(1000);
                retries--;
            }

            return false;
        }

        public async Task<bool> BuildConnection(string ipAddress = null, int retries = 3)
        {
            while (retries > 0)
            {
                _logger.LogInformation($"{nameof(BuildConnection)}: Retrieving server's ip address with retries = {retries}...");
                if (string.IsNullOrWhiteSpace(ipAddress))
                {
                    ipAddress = WebServerUtils.GetWebServerIpAddress();
                }
                else
                {
                    _logger.LogInformation($"{nameof(BuildConnection)}: Ip address = {ipAddress} was provided, using that one to try to connect to the server's hub...");
                }

                if (!string.IsNullOrWhiteSpace(ipAddress))
                {
                    if (ipAddress.EndsWith("/"))
                    {
                        ipAddress = ipAddress[..^1];
                    }

                    if (_connection != null)
                    {
                        _connection.Closed -= ConnectionClosed;
                        await _connection.DisposeAsync();
                    }

                    //TODO: MOVE THE CastItHub TO A CONSTANT 
                    var url = $"{ipAddress}/CastItHub";
                    _connection = new HubConnectionBuilder()
                        .WithUrl(url)
                        .Build();

                    _connection.Closed += ConnectionClosed;
                    _logger.LogInformation($"{nameof(BuildConnection)}: The server's hub address is = {url}");
                    IpAddress = ipAddress;
                    return true;
                }

                await Task.Delay(1000);
                retries--;
            }

            return false;
        }

        public async Task<bool> ConnectToHub(int retries = 5)
        {
            while (retries > 0)
            {
                try
                {
                    _logger.LogInformation($"{nameof(ConnectToHub)}: Trying to connect to server's hub with retries = {retries}...");
                    await _connection.StartAsync();
                    _logger.LogInformation($"{nameof(ConnectToHub)}: Connection was successfully established");
                    return true;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"{nameof(ConnectToHub)}: Unknown error occurred");
                }

                await Task.Delay(500);
                retries--;
            }
            return false;
        }

        private void ListenPlayerEvents()
        {
            _connection.On<ServerPlayerStatusResponseDto>("PlayerStatusChanged", status => OnPlayerStatusChanged?.Invoke(status));

            _connection.On<ServerAppSettings>("PlayerSettingsChanged", settings => OnPlayerSettingsChanged?.Invoke(settings));

            _connection.On<AppMessageType>("ServerMessage", msg => OnServerMessage?.Invoke(msg));

            _connection.On<Receiver>("CastDeviceSet", device => OnCastDeviceSet?.Invoke(device));

            _connection.On<List<Receiver>>("CastDevicesChanged", devices => OnCastDevicesChanged?.Invoke(devices.Cast<IReceiver>().ToList()));

            _connection.On("CastDeviceDisconnected", () => OnCastDeviceDisconnected?.Invoke());

            _connection.On("StoppedPlayBack", () => OnStoppedPlayback?.Invoke());
        }

        private void ListenPlayListsEvents()
        {
            _connection.On<List<GetAllPlayListResponseDto>>("SendPlayLists", playLists => OnPlayListsLoaded?.Invoke(playLists));

            _connection.On<GetAllPlayListResponseDto>("PlayListAdded", playList => OnPlayListAdded?.Invoke(playList));

            _connection.On<List<GetAllPlayListResponseDto>>("PlayListsChanged", playLists => OnPlayListsChanged?.Invoke(playLists));

            _connection.On<GetAllPlayListResponseDto>("PlayListChanged", playList => OnPlayListChanged?.Invoke(playList));

            _connection.On<long>("PlayListDeleted", id => OnPlayListDeleted?.Invoke(id));

            _connection.On<long, bool>("PlayListIsBusy", (id, isBusy) => OnPlayListBusy?.Invoke(id, isBusy));
        }

        private void ListenFileEvents()
        {
            _connection.On<FileItemResponseDto>("FileAdded", file => OnFileAdded?.Invoke(file));

            _connection.On<FileItemResponseDto>("FileChanged", file => OnFileChanged?.Invoke(file));

            _connection.On<List<FileItemResponseDto>>("FilesChanged", files => OnFilesChanged?.Invoke(files));

            _connection.On<long, long>("FileDeleted", (playListId, id) => OnFileDeleted?.Invoke(playListId, id));

            _connection.On<FileItemResponseDto>("FileLoading", file => OnFileLoading?.Invoke(file));

            _connection.On<FileItemResponseDto>("FileLoaded", file => OnFileLoaded?.Invoke(file));

            _connection.On<FileItemResponseDto>("FileEndReached", file => OnFileEndReached?.Invoke(file));
        }

        private Task ConnectionClosed(Exception ex)
        {
            if (ex != null)
            {
                _logger.LogError(ex, $"{nameof(ConnectionClosed)}: Connection closed");
            }
            OnClientDisconnected?.Invoke();
            return Task.CompletedTask;
        }
        #endregion
    }
}
