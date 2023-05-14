import 'dart:async';

import 'package:castit/domain/enums/enums.dart';
import 'package:castit/domain/models/models.dart';
import 'package:tuple/tuple.dart';

abstract class CastItHubClientService {
  StreamController<void> get connected;

  StreamController<void> get fileLoading;

  StreamController<PlayedFile> get fileLoaded;

  StreamController<String> get fileLoadingError;

  StreamController<double> get fileTimeChanged;

  StreamController<void> get filePaused;

  StreamController<void> get fileEndReached;

  StreamController<void> get disconnected;

  StreamController<void> get appClosing;

  StreamController<ServerAppSettings> get settingsChanged;

  StreamController<PlayListItemResponseDto?> get playlistLoaded;

  StreamController<List<FileItemOptionsResponseDto>> get fileOptionsLoaded;

  StreamController<VolumeLevelChangedResponseDto> get volumeLevelChanged;

  StreamController<RefreshPlayListResponseDto> get refreshPlayList;

  StreamController<AppMessageType> get serverMessageReceived;

  StreamController<GetAllPlayListResponseDto> get playListAdded;

  StreamController<List<GetAllPlayListResponseDto>> get playListsChanged;

  StreamController<Tuple2<bool, GetAllPlayListResponseDto>> get playListChanged;

  StreamController<int> get playListDeleted;

  StreamController<FileItemResponseDto> get fileAdded;

  StreamController<Tuple2<bool, FileItemResponseDto>> get fileChanged;

  StreamController<List<FileItemResponseDto>> get filesChanged;

  StreamController<Tuple2<int, int>> get fileDeleted;

  bool get isConnected;

  Future<void> connectToHub();

  Future<void> dispose();

  Future<void> disconnectFromHub({bool triggerEvent = true});

  Future<void> playFile(int id, int playListId, {bool force = false});

  Future<void> gotoSeconds(double seconds);

  Future<void> skipSeconds(double seconds);

  Future<void> goTo({bool next = false, bool previous = false});

  Future<void> togglePlayBack();

  Future<void> stopPlayBack();

  Future<void> setPlayListOptions(int id, {bool loop = false, bool shuffle = false});

  Future<void> deletePlayList(int id);

  Future<void> deleteFile(int id, int playListId);

  Future<void> loopFile(int id, int playListId, {bool loop = false});

  Future<void> setFileOptions(int streamIndex, {bool isAudio = false, bool isSubtitle = false, bool isQuality = false});

  Future<void> updateSettings(ServerAppSettings dto);

  Future<void> loadPlayLists();

  Future<PlayListItemResponseDto> loadPlayList(int playListId);

  Future<void> setVolume(double volumeLvl, {bool isMuted = false});

  Future<void> updatePlayList(int id, String name);
}
