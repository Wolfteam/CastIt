import { HubConnection, HubConnectionBuilder, HubConnectionState } from "@microsoft/signalr";
import { AppMessage, SortMode } from "../enums";
import { Subject } from "rxjs";
import {
    IAddFolderOrFilesToPlayListRequestDto,
    IAddUrlToPlayListRequestDto,
    IFileDeleted,
    IFileItemResponseDto,
    IGetAllPlayListResponseDto,
    IPlayFileRequestDto,
    IPlayListBusy,
    IPlayListItemResponseDto,
    IReceiver,
    IServerAppSettings,
    IServerPlayerStatusResponseDto,
    ISetFileOptionsRequestDto,
    ISetPlayListOptionsRequestDto,
    ISetVolumeRequestDto,
    IUpdatePlayListRequestDto,
} from "../models";

export const onClientConnected = new Subject<void>();
export const onClientDisconnected = new Subject<void>();

export const onPlayerStatusChanged = new Subject<IServerPlayerStatusResponseDto>();
export const onPlayerSettingsChanged = new Subject<IServerAppSettings>();
export const onCastDeviceSet = new Subject<IReceiver>();
export const onCastDevicesChanged = new Subject<IReceiver[]>();
export const onCastDeviceDisconnected = new Subject<void>();
export const onServerMessage = new Subject<AppMessage>();
export const onStoppedPlayback = new Subject<void>();

export const onPlayListsLoaded = new Subject<IGetAllPlayListResponseDto[]>();
export const onPlayListAdded = new Subject<IGetAllPlayListResponseDto>();
export const onPlayListChanged = new Subject<IGetAllPlayListResponseDto>();
export const onPlayListsChanged = new Subject<IGetAllPlayListResponseDto[]>();
export const onPlayListDeleted = new Subject<number>();
export const onPlayListBusy = new Subject<IPlayListBusy>();

export const onFileAdded = new Subject<IFileItemResponseDto>();
export const onFileChanged = new Subject<IFileItemResponseDto>();
export const onFilesChanged = new Subject<IFileItemResponseDto[]>();
export const onFileDeleted = new Subject<IFileDeleted>();
export const onFileLoading = new Subject<IFileItemResponseDto>();
export const onFileLoaded = new Subject<IFileItemResponseDto>();
export const onFileEndReached = new Subject<IFileItemResponseDto>();

let _connection: HubConnection | null = null;

export const initializeHubConnection = async (): Promise<boolean> => {
    await stop();

    _connection = new HubConnectionBuilder().withUrl(process.env.REACT_APP_BASE_HUB_URL!).build();

    _connection.onclose(_onConnectionClosed);

    _subscribeToEvents(_connection);
    try {
        await _connection.start();

        if (_connection.state === HubConnectionState.Connected) {
            onClientConnected.next();
        }
        return true;
    } catch (error) {
        console.log(error);
        return false;
    }
};

const stop = async (): Promise<void> => {
    if (_connection) {
        await _connection.stop();
        _connection = null;
    }
};

const _onConnectionClosed = (error?: Error) => {
    if (error) {
        console.log(error);
    }
    onClientDisconnected.next();
};

const _subscribeToEvents = (connection: HubConnection): void => {
    //player
    connection.on("PlayerStatusChanged", (status: IServerPlayerStatusResponseDto) =>
        onPlayerStatusChanged.next(status)
    );
    connection.on("PlayerSettingsChanged", (settings: IServerAppSettings) => onPlayerSettingsChanged.next(settings));
    connection.on("ServerMessage", (msg: AppMessage) => onServerMessage.next(msg));
    connection.on("CastDeviceSet", (device: IReceiver) => onCastDeviceSet.next(device));
    connection.on("CastDevicesChanged", (devices: IReceiver[]) => onCastDevicesChanged.next(devices));
    connection.on("CastDeviceDisconnected", () => onCastDeviceDisconnected.next());
    connection.on("StoppedPlayBack", () => onStoppedPlayback.next());

    //playlists
    connection.on("SendPlayLists", (playLists: IGetAllPlayListResponseDto[]) => onPlayListsLoaded.next(playLists));
    connection.on("PlayListAdded", (playList: IGetAllPlayListResponseDto) => onPlayListAdded.next(playList));
    connection.on("PlayListsChanged", (playLists: IGetAllPlayListResponseDto[]) => onPlayListsChanged.next(playLists));
    connection.on("PlayListChanged", (playList: IGetAllPlayListResponseDto) => onPlayListChanged.next(playList));
    connection.on("PlayListDeleted", (id: number) => onPlayListDeleted.next(id));
    connection.on("PlayListIsBusy", (id: number, isBusy: boolean) =>
        onPlayListBusy.next({
            playListId: id,
            isBusy: isBusy,
        })
    );

    //files
    connection.on("FileAdded", (file: IFileItemResponseDto) => onFileAdded.next(file));
    connection.on("FileChanged", (file: IFileItemResponseDto) => onFileChanged.next(file));
    connection.on("FilesChanged", (files: IFileItemResponseDto[]) => onFilesChanged.next(files));
    connection.on("FileDeleted", (playListId: number, id: number) =>
        onFileDeleted.next({
            playListId: playListId,
            fileId: id,
        })
    );
    connection.on("FileLoading", (file: IFileItemResponseDto) => onFileLoading.next(file));
    connection.on("FileLoaded", (file: IFileItemResponseDto) => onFileLoaded.next(file));
    connection.on("FileEndReached", (file: IFileItemResponseDto) => onFileEndReached.next(file));
};

const _send = async <T = any>(methodName: string, ...args: any[]): Promise<T | null> => {
    try {
        if (!_connection) {
            await initializeHubConnection();
        }

        if (args.length) {
            return await _connection!.invoke<T>(methodName, ...args);
        }
        return await _connection!.invoke<T>(methodName);
    } catch (error) {
        console.error(`Unknown error occurred while trying to call hub method = ${methodName}`, error);
        return null;
    }
};

//Player methods
export const play = async (
    playListId: number,
    fileId: number,
    force: boolean,
    fileOptionsChanged: boolean = false
): Promise<void> => {
    const request: IPlayFileRequestDto = {
        playListId: playListId,
        id: fileId,
        force: force,
        fileOptionsChanged: fileOptionsChanged,
    };
    await _send("Play", request);
};

export const gotoPosition = async (position: number): Promise<void> => {
    await _send("GoToPosition", position);
};

export const skipSeconds = async (seconds: number): Promise<void> => {
    await _send("SkipSeconds", seconds);
};

export const goTo = async (next: boolean, previous: boolean): Promise<void> => {
    await _send("GoTo", next, previous);
};

export const togglePlayBack = async (): Promise<void> => {
    await _send("TogglePlayBack");
};

export const stopPlayBack = async (): Promise<void> => {
    await _send("StopPlayBack");
};

export const setVolume = async (level: number, isMuted: boolean): Promise<void> => {
    const request: ISetVolumeRequestDto = {
        volumeLevel: level,
        isMuted: isMuted,
    };
    await _send("SetVolume", request);
};

export const updateSettings = async (settings: IServerAppSettings): Promise<void> => {
    await _send("UpdateSettings", settings);
};

export const connectToCastDevice = async (id: string | null): Promise<void> => {
    await _send("ConnectToCastDevice", id);
};

export const refreshCastDevices = async (): Promise<void> => {
    await _send("RefreshCastDevices", null);
};

export const setFileSubtitlesFromPath = async (path: string): Promise<void> => {
    await _send("SetFileSubtitlesFromPath", path);
};

//Playlist methods
export const addNewPlayList = async (): Promise<IPlayListItemResponseDto> => {
    const playList = await _send<IPlayListItemResponseDto>("AddNewPlayList");
    return playList!;
};

export const getPlayList = async (id: number): Promise<IPlayListItemResponseDto> => {
    const playList = await _send<IPlayListItemResponseDto>("GetPlayList", id);
    return playList!;
};

export const updatePlayList = async (id: number, name: string) => {
    const request: IUpdatePlayListRequestDto = {
        name: name,
    };
    await _send("UpdatePlayList", id, request);
};

export const updatePlayListPosition = async (playListId: number, newIndex: number) => {
    await _send("UpdatePlayListPosition", playListId, newIndex);
};

export const setPlayListOptions = async (playListId: number, loop: boolean, shuffle: boolean): Promise<void> => {
    const request: ISetPlayListOptionsRequestDto = {
        loop: loop,
        shuffle: shuffle,
    };
    await _send("SetPlayListOptions", playListId, request);
};

export const deletePlayList = async (id: number): Promise<void> => {
    await _send("DeletePlayList", id);
};

export const deleteAllPlayLists = async (exceptId: number): Promise<void> => {
    await _send("DeleteAllPlayLists", exceptId);
};

export const removeFiles = async (playListId: number, ids: number[]): Promise<void> => {
    await _send("RemoveFiles", playListId, ids);
};

export const removeFilesThatStartsWith = async (playListId: number, path: string): Promise<void> => {
    await _send("RemoveFilesThatStartsWith", playListId, path);
};

export const removeAllMissingFiles = async (playListId: number): Promise<void> => {
    await _send("RemoveAllMissingFiles", playListId);
};

export const addFolders = async (
    playListId: number,
    includeSubFolder: boolean,
    ...folders: string[]
): Promise<void> => {
    const request: IAddFolderOrFilesToPlayListRequestDto = {
        folders: folders,
        includeSubFolders: includeSubFolder,
        files: [],
    };
    await _send("AddFolders", playListId, request);
};

export const addFiles = async (playListId: number, ...files: string[]): Promise<void> => {
    const request: IAddFolderOrFilesToPlayListRequestDto = {
        folders: [],
        includeSubFolders: false,
        files: files,
    };
    await _send("AddFiles", playListId, request);
};

export const addUrlFile = async (playListId: number, url: string, onlyVideo: boolean): Promise<void> => {
    const request: IAddUrlToPlayListRequestDto = {
        url: url,
        onlyVideo: onlyVideo,
    };
    await _send("AddUrlFile", playListId, request);
};

export const sortFiles = async (playListId: number, sortMode: SortMode): Promise<void> => {
    await _send("SortFiles", playListId, sortMode);
};

//File methods
export const loopFile = async (playListId: number, id: number, loop: boolean): Promise<void> => {
    await _send("LoopFile", playListId, id, loop);
};

export const deleteFile = async (playListId: number, id: number): Promise<void> => {
    await _send("DeleteFile", playListId, id);
};

export const setFileOptions = async (
    streamIndex: number,
    isAudio: boolean,
    isSubTitle: boolean,
    isQuality: boolean
): Promise<void> => {
    const request: ISetFileOptionsRequestDto = {
        isAudio: isAudio,
        isQuality: isQuality,
        isSubTitle: isSubTitle,
        streamIndex: streamIndex,
    };
    await _send("SetFileOptions", request);
};

export const updateFilePosition = async (playListId: number, id: number, newIndex: number): Promise<void> => {
    await _send("UpdateFilePosition", playListId, id, newIndex);
};
