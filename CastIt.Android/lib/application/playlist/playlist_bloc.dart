import 'dart:async';

import 'package:bloc/bloc.dart';
import 'package:castit/domain/extensions/string_extensions.dart';
import 'package:castit/domain/models/models.dart';
import 'package:castit/domain/services/castit_hub_client_service.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter/widgets.dart';
import 'package:freezed_annotation/freezed_annotation.dart';
import 'package:tuple/tuple.dart';

part 'playlist_bloc.freezed.dart';
part 'playlist_event.dart';
part 'playlist_state.dart';

class PlayListBloc extends Bloc<PlayListEvent, PlayListState> {
  final CastItHubClientService _castItHub;
  final List<StreamSubscription> _subscriptions = [];

  PlayListState get initialState => PlayListState.loading();

  _LoadedState get currentState => state as _LoadedState;

  PlayListBloc(this._castItHub) : super(PlayListState.loading());

  @override
  Stream<PlayListState> mapEventToState(PlayListEvent event) async* {
    if (event is _Load || event is _Loaded) {
      //If there were playlists loaded, hide them all!
      yield initialState;
    }

    final s = event.map(
      disconnected: (e) async => PlayListState.disconnected(playListId: e.playListId),
      load: (e) async {
        final playList = await _castItHub.loadPlayList(e.id);
        return PlayListState.loaded(
          playlistId: playList.id,
          name: playList.name,
          loop: playList.loop,
          position: playList.position,
          shuffle: playList.shuffle,
          files: playList.files,
          loaded: true,
          scrollToFileId: e.scrollToFileId,
        );
      },
      loaded: (e) async => PlayListState.loaded(
        playlistId: e.playlist.id,
        name: e.playlist.name,
        loop: e.playlist.loop,
        position: e.playlist.position,
        shuffle: e.playlist.shuffle,
        files: e.playlist.files,
        loaded: true,
      ),
      playListOptionsChanged: (e) async => currentState.copyWith(loop: e.loop, shuffle: e.shuffle),
      toggleSearchBoxVisibility: (e) async => currentState.copyWith(
        searchBoxIsVisible: !currentState.searchBoxIsVisible,
      ),
      searchBoxTextChanged: (e) async {
        final isFiltering = !e.text.isNullEmptyOrWhitespace;
        final filteredFiles = !isFiltering
            ? <FileItemResponseDto>[]
            : currentState.files.where((element) => element.filename.toLowerCase().contains(e.text.toLowerCase())).toList();

        return currentState.copyWith(filteredFiles: filteredFiles, isFiltering: isFiltering);
      },
      closePage: (e) async => PlayListState.close(),
      notFound: (_) async => PlayListState.notFound(),
      playListChanged: (e) async => _handlePlayListChanged(e.playList),
      playListDeleted: (e) async => _handlePlayListDeleted(e.id),
      fileAdded: (e) async => _handleFileAdded(e.file),
      filesChanged: (e) async => _handleFilesChanged(e.files),
      fileChanged: (e) async => _handleFileChanged(e.file),
      fileDeleted: (e) async => _handleFileDeleted(e.playListId, e.id),
    );

    yield await s;

    //Clear any previous scrolled file
    if (state is _LoadedState && currentState.scrollToFileId != null) {
      yield currentState.copyWith.call(scrollToFileId: null);
    }
  }

  @override
  Future<void> close() async {
    await Future.wait(_subscriptions.map((e) => e.cancel()).toList());
    return super.close();
  }

  void listenHubEvents() {
    _subscriptions.addAll([
      _castItHub.playlistLoaded.stream.listen(_onPlayListLoaded),
      _castItHub.connected.stream.listen((_) => _onConnected),
      _castItHub.disconnected.stream.listen((_) => _onDisconnected()),
      _castItHub.refreshPlayList.stream.listen(_onRefresh),
      _castItHub.playListsChanged.stream.listen(_onPlayListsChanged),
      _castItHub.playListChanged.stream.listen(_onPlayListChanged),
      _castItHub.playListDeleted.stream.listen(_onPlayListDeleted),
      _castItHub.fileAdded.stream.listen(_onFileAdded),
      _castItHub.fileChanged.stream.listen(_onFileChanged),
      _castItHub.filesChanged.stream.listen(_onFilesChanged),
      _castItHub.fileDeleted.stream.listen(_onFileDeleted),
    ]);
  }

  PlayListState _handlePlayListChanged(GetAllPlayListResponseDto playList) {
    return state.maybeMap(
      loaded: (s) {
        if (s.playlistId != playList.id) {
          return s;
        }

        return s.copyWith.call(
          shuffle: playList.shuffle,
          name: playList.name,
          position: playList.position,
          loop: playList.loop,
        );
      },
      orElse: () => state,
    );
  }

  PlayListState _handlePlayListDeleted(int id) {
    return state.maybeMap(
      loaded: (s) {
        if (s.playlistId != id) {
          return s;
        }

        return PlayListState.close();
      },
      orElse: () => state,
    );
  }

  PlayListState _handleFileAdded(FileItemResponseDto file) {
    return state.maybeMap(
      loaded: (s) {
        if (s.playlistId != file.playListId) {
          return s;
        }

        final files = [...s.files];
        files.insert(file.position - 1, file);

        return s.copyWith.call(files: files);
      },
      orElse: () => state,
    );
  }

  PlayListState _handleFilesChanged(List<FileItemResponseDto> files) {
    return state.maybeMap(
      loaded: (s) => s.copyWith.call(files: files),
      orElse: () => state,
    );
  }

  PlayListState _handleFileChanged(FileItemResponseDto file) {
    return state.maybeMap(
      loaded: (s) {
        if (s.playlistId != file.playListId) {
          return s;
        }

        final files = [...s.files];
        final current = files.firstWhereOrNull((el) => el.id == file.id);
        if (current == null || file == current) {
          return s;
        }
        final updated = current.copyWith.call(
          loop: file.loop,
          position: file.position,
          name: file.name,
          totalDuration: file.totalDuration,
          playedTime: file.playedTime,
          size: file.size,
          canStartPlayingFromCurrentPercentage: file.canStartPlayingFromCurrentPercentage,
          description: file.description,
          exists: file.exists,
          extension: file.extension,
          filename: file.filename,
          fullTotalDuration: file.fullTotalDuration,
          isBeingPlayed: file.isBeingPlayed,
          isCached: file.isCached,
          isLocalFile: file.isLocalFile,
          isUrlFile: file.isUrlFile,
          path: file.path,
          playedPercentage: file.playedPercentage,
          playedSeconds: file.playedSeconds,
          playListId: file.playListId,
          subTitle: file.subTitle,
          thumbnailUrl: file.thumbnailUrl,
          totalSeconds: file.totalSeconds,
          wasPlayed: file.wasPlayed,
          currentFileVideos: file.currentFileVideos,
          currentFileAudios: file.currentFileAudios,
          currentFileSubTitles: file.currentFileSubTitles,
          currentFileQualities: file.currentFileQualities,
          currentFileVideoStreamIndex: file.currentFileVideoStreamIndex,
          currentFileAudioStreamIndex: file.currentFileAudioStreamIndex,
          currentFileSubTitleStreamIndex: file.currentFileSubTitleStreamIndex,
          currentFileQuality: file.currentFileQuality,
        );
        final currentIndex = files.indexOf(current);
        files.removeAt(currentIndex);
        files.insert(currentIndex, updated);

        if (file.isBeingPlayed) {
          final otherPlayedFile = files.where((f) => f.id != file.id && f.isBeingPlayed).toList();
          for (var i = 0; i < otherPlayedFile.length; i++) {
            final other = otherPlayedFile[i];
            final otherIndex = files.indexOf(other);
            final otherUpdated = other.copyWith.call(isBeingPlayed: false);
            files.removeAt(otherIndex);
            files.insert(otherIndex, otherUpdated);
          }
        }

        return s.copyWith.call(files: files);
      },
      orElse: () => state,
    );
  }

  PlayListState _handleFileDeleted(int playListId, int id) {
    return state.maybeMap(
      loaded: (s) {
        if (s.playlistId != playListId) {
          return s;
        }

        final files = [...s.files];
        files.removeWhere((el) => el.id == id);
        return s.copyWith.call(files: files);
      },
      orElse: () => state,
    );
  }

  void _onPlayListLoaded(PlayListItemResponseDto? event) {
    //Playlist does not exist
    if (event == null) {
      add(const PlayListEvent.notFound());
      return;
    }

    if (state is! _LoadedState || currentState.playlistId == event.id) {
      add(PlayListEvent.loaded(playlist: event));
      return;
    }
  }

  void _onConnected() {
    state.maybeMap(
      disconnected: (state) {
        if (state.playListId != null) {
          add(PlayListEvent.load(id: state.playListId!));
        }
      },
      orElse: () {},
    );
  }

  void _onDisconnected() {
    state.maybeMap(
      loaded: (s) => add(PlayListEvent.disconnected(playListId: s.playlistId)),
      orElse: () => add(const PlayListEvent.disconnected()),
    );
  }

  void _onRefresh(RefreshPlayListResponseDto event) {
    if (state is! _LoadedState || currentState.playlistId != event.id) {
      return;
    }
    if (!event.wasDeleted) {
      add(PlayListEvent.load(id: event.id));
    } else {
      add(const PlayListEvent.closePage());
    }
  }

  void _onPlayListsChanged(List<GetAllPlayListResponseDto> event) {
    state.maybeMap(
      loaded: (s) {
        final playList = event.firstWhereOrNull((el) => el.id == s.playlistId);
        if (playList == null) {
          return;
        }

        add(PlayListEvent.playListChanged(playList: playList));
      },
      orElse: () {},
    );
  }

  void _onPlayListChanged(Tuple2<bool, GetAllPlayListResponseDto> event) {
    state.maybeMap(
      loaded: (_) {
        final changeComesFromPlayedFile = event.item1;
        final playList = event.item2;
        if (!changeComesFromPlayedFile) {
          add(PlayListEvent.playListChanged(playList: playList));
        }
      },
      orElse: () {},
    );
  }

  void _onPlayListDeleted(int event) {
    state.maybeMap(
      loaded: (_) => add(PlayListEvent.playListDeleted(id: event)),
      orElse: () {},
    );
  }

  void _onFileAdded(FileItemResponseDto event) {
    state.maybeMap(
      loaded: (_) => add(PlayListEvent.fileAdded(file: event)),
      orElse: () {},
    );
  }

  void _onFileChanged(Tuple2<bool, FileItemResponseDto> event) {
    state.maybeMap(
      loaded: (state) {
        //The second part of the if is to make sure that we only trigger the change
        // if we had a previously played file
        final changeComesFromPlayedFile = event.item1;
        final file = event.item2;
        final currentPlayedFile = state.files.firstWhereOrNull((el) => el.isBeingPlayed);
        if (!changeComesFromPlayedFile || currentPlayedFile?.id != file.id) {
          add(PlayListEvent.fileChanged(file: file));
        }
      },
      orElse: () {},
    );
  }

  void _onFilesChanged(List<FileItemResponseDto> event) {
    state.maybeMap(
      loaded: (_) => add(PlayListEvent.filesChanged(files: event)),
      orElse: () {},
    );
  }

  void _onFileDeleted(Tuple2<int, int> event) {
    state.maybeMap(
      loaded: (_) => add(PlayListEvent.fileDeleted(playListId: event.item1, id: event.item2)),
      orElse: () {},
    );
  }
}
