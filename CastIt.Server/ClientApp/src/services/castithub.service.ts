import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { AppMessage, SortMode } from '../enums';
import { BehaviorSubject, Subject } from 'rxjs';
import {
    IAddFolderOrFileOrUrlToPlayListRequestDto,
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
} from '../models';

export const onClientConnected = new Subject<void>();
export const onClientDisconnected = new Subject<void>();

export const onPlayerStatusChanged = new BehaviorSubject<IServerPlayerStatusResponseDto | null>(null);
export const onPlayerSettingsChanged = new BehaviorSubject<IServerAppSettings | null>(null);
export const onCastDeviceSet = new Subject<IReceiver>();
export const onCastDevicesChanged = new BehaviorSubject<IReceiver[]>([]);
export const onCastDeviceDisconnected = new Subject<void>();
export const onServerMessage = new Subject<AppMessage>();
export const onStoppedPlayback = new Subject<void>();

export const onPlayListsLoaded = new BehaviorSubject<IGetAllPlayListResponseDto[]>([]);
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

export class CastItHubService {
    private connection: HubConnection | null = null;

    async connect(): Promise<void> {
        await this.disconnect();

        this.connection = new HubConnectionBuilder().withUrl(process.env.REACT_APP_BASE_HUB_URL!).build();

        this.connection.onclose(this.onConnectionClosed);

        this.subscribeToEvents(this.connection);
        try {
            await this.connection.start();

            if (this.connection.state === HubConnectionState.Connected) {
                onClientConnected.next();
            }
        } catch (error) {
            console.log(error);
            throw error;
        }
    }

    async disconnect(): Promise<void> {
        if (this.connection) {
            await this.connection.stop();
            this.connection = null;
        }
    }

    //Player methods
    async play(playListId: number, fileId: number, force: boolean, fileOptionsChanged: boolean = false): Promise<void> {
        const request: IPlayFileRequestDto = {
            playListId: playListId,
            id: fileId,
            force: force,
            fileOptionsChanged: fileOptionsChanged,
        };
        await this.send('Play', request);
    }

    gotoSeconds = async (seconds: number): Promise<void> => {
        await this.send('GoToSeconds', seconds);
    };

    gotoPosition = async (position: number): Promise<void> => {
        await this.send('GoToPosition', position);
    };

    skipSeconds = async (seconds: number): Promise<void> => {
        await this.send('SkipSeconds', seconds);
    };

    goTo = async (next: boolean, previous: boolean): Promise<void> => {
        await this.send('GoTo', next, previous);
    };

    togglePlayBack = async (): Promise<void> => {
        await this.send('TogglePlayBack');
    };

    stopPlayBack = async (): Promise<void> => {
        await this.send('StopPlayBack');
    };

    setVolume = async (level: number, isMuted: boolean): Promise<void> => {
        const request: ISetVolumeRequestDto = {
            volumeLevel: level,
            isMuted: isMuted,
        };
        await this.send('SetVolume', request);
    };

    updateSettings = async (settings: IServerAppSettings): Promise<void> => {
        await this.send('UpdateSettings', settings);
    };

    connectToCastDevice = async (id: string | null): Promise<void> => {
        await this.send('ConnectToCastDevice', id);
    };

    refreshCastDevices = async (): Promise<void> => {
        await this.send('RefreshCastDevices', null);
    };

    setFileSubtitlesFromPath = async (path: string): Promise<void> => {
        await this.send('SetFileSubtitlesFromPath', path);
    };

    //Playlist methods
    addNewPlayList = async (): Promise<IPlayListItemResponseDto> => {
        const playList = await this.send<IPlayListItemResponseDto>('AddNewPlayList');
        return playList!;
    };

    getPlayList = async (id: number): Promise<IPlayListItemResponseDto> => {
        const playList = await this.send<IPlayListItemResponseDto>('GetPlayList', id);
        return playList!;
    };

    updatePlayList = async (id: number, name: string) => {
        const request: IUpdatePlayListRequestDto = {
            name: name,
        };
        await this.send('UpdatePlayList', id, request);
    };

    updatePlayListPosition = async (playListId: number, newIndex: number) => {
        await this.send('UpdatePlayListPosition', playListId, newIndex);
    };

    setPlayListOptions = async (playListId: number, loop: boolean, shuffle: boolean): Promise<void> => {
        const request: ISetPlayListOptionsRequestDto = {
            loop: loop,
            shuffle: shuffle,
        };
        await this.send('SetPlayListOptions', playListId, request);
    };

    deletePlayList = async (id: number): Promise<void> => {
        await this.send('DeletePlayList', id);
    };

    deleteAllPlayLists = async (exceptId: number): Promise<void> => {
        await this.send('DeleteAllPlayLists', exceptId);
    };

    removeFiles = async (playListId: number, ids: number[]): Promise<void> => {
        await this.send('RemoveFiles', playListId, ids);
    };

    removeFilesThatStartsWith = async (playListId: number, path: string): Promise<void> => {
        await this.send('RemoveFilesThatStartsWith', playListId, path);
    };

    removeAllMissingFiles = async (playListId: number): Promise<void> => {
        await this.send('RemoveAllMissingFiles', playListId);
    };

    addFolders = async (playListId: number, includeSubFolder: boolean, ...folders: string[]): Promise<void> => {
        const request: IAddFolderOrFilesToPlayListRequestDto = {
            folders: folders,
            includeSubFolders: includeSubFolder,
            files: [],
        };
        await this.send('AddFolders', playListId, request);
    };

    addFiles = async (playListId: number, ...files: string[]): Promise<void> => {
        const request: IAddFolderOrFilesToPlayListRequestDto = {
            folders: [],
            includeSubFolders: false,
            files: files,
        };
        await this.send('AddFiles', playListId, request);
    };

    addUrlFile = async (playListId: number, url: string, onlyVideo: boolean): Promise<void> => {
        const request: IAddUrlToPlayListRequestDto = {
            url: url,
            onlyVideo: onlyVideo,
        };
        await this.send('AddUrlFile', playListId, request);
    };

    addFolderOrFileOrUrl = async (playListId: number, path: string, includeSubFolder: boolean, onlyVideo: boolean): Promise<void> => {
        const request: IAddFolderOrFileOrUrlToPlayListRequestDto = {
            path: path,
            includeSubFolders: includeSubFolder,
            onlyVideo: onlyVideo,
        };
        await this.send('AddFolderOrFileOrUrl', playListId, request);
    };

    sortFiles = async (playListId: number, sortMode: SortMode): Promise<void> => {
        await this.send('SortFiles', playListId, sortMode);
    };

    //File methods
    loopFile = async (playListId: number, id: number, loop: boolean): Promise<void> => {
        await this.send('LoopFile', playListId, id, loop);
    };

    deleteFile = async (playListId: number, id: number): Promise<void> => {
        await this.send('DeleteFile', playListId, id);
    };

    setFileOptions = async (streamIndex: number, isAudio: boolean, isSubTitle: boolean, isQuality: boolean): Promise<void> => {
        const request: ISetFileOptionsRequestDto = {
            isAudio: isAudio,
            isQuality: isQuality,
            isSubTitle: isSubTitle,
            streamIndex: streamIndex,
        };
        await this.send('SetFileOptions', request);
    };

    updateFilePosition = async (playListId: number, id: number, newIndex: number): Promise<void> => {
        await this.send('UpdateFilePosition', playListId, id, newIndex);
    };

    private async send<T = any>(methodName: string, ...args: any[]): Promise<T | null> {
        try {
            if (!this.connection) {
                await this.connect();
            }

            if (args.length) {
                return await this.connection!.invoke<T>(methodName, ...args);
            }
            return await this.connection!.invoke<T>(methodName);
        } catch (error) {
            console.error(`Unknown error occurred while trying to call hub method = ${methodName}`, error);
            return null;
        }
    }

    private onConnectionClosed(error?: Error): void {
        if (error) {
            console.log(error);
        }
        onClientDisconnected.next();
    }

    private subscribeToEvents(connection: HubConnection): void {
        //player
        connection.on('PlayerStatusChanged', (status: IServerPlayerStatusResponseDto) => onPlayerStatusChanged.next(status));
        connection.on('PlayerSettingsChanged', (settings: IServerAppSettings) => onPlayerSettingsChanged.next(settings));
        connection.on('ServerMessage', (msg: AppMessage) => onServerMessage.next(msg));
        connection.on('CastDeviceSet', (device: IReceiver) => onCastDeviceSet.next(device));
        connection.on('CastDevicesChanged', (devices: IReceiver[]) => onCastDevicesChanged.next(devices));
        connection.on('CastDeviceDisconnected', () => onCastDeviceDisconnected.next());
        connection.on('StoppedPlayBack', () => onStoppedPlayback.next());

        //playlists
        connection.on('SendPlayLists', (playLists: IGetAllPlayListResponseDto[]) => onPlayListsLoaded.next(playLists));
        connection.on('PlayListAdded', (playList: IGetAllPlayListResponseDto) => onPlayListAdded.next(playList));
        connection.on('PlayListsChanged', (playLists: IGetAllPlayListResponseDto[]) => onPlayListsChanged.next(playLists));
        connection.on('PlayListChanged', (playList: IGetAllPlayListResponseDto) => onPlayListChanged.next(playList));
        connection.on('PlayListDeleted', (id: number) => onPlayListDeleted.next(id));
        connection.on('PlayListIsBusy', (id: number, isBusy: boolean) =>
            onPlayListBusy.next({
                playListId: id,
                isBusy: isBusy,
            })
        );

        //files
        connection.on('FileAdded', (file: IFileItemResponseDto) => onFileAdded.next(file));
        connection.on('FileChanged', (file: IFileItemResponseDto) => onFileChanged.next(file));
        connection.on('FilesChanged', (files: IFileItemResponseDto[]) => onFilesChanged.next(files));
        connection.on('FileDeleted', (playListId: number, id: number) =>
            onFileDeleted.next({
                playListId: playListId,
                fileId: id,
            })
        );
        connection.on('FileLoading', (file: IFileItemResponseDto) => onFileLoading.next(file));
        connection.on('FileLoaded', (file: IFileItemResponseDto) => onFileLoaded.next(file));
        connection.on('FileEndReached', (file: IFileItemResponseDto) => onFileEndReached.next(file));
    }
}
