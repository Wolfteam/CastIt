import 'dart:async';

import 'package:bloc/bloc.dart';
import 'package:castit/domain/extensions/string_extensions.dart';
import 'package:castit/domain/models/models.dart';
import 'package:castit/domain/services/castit_hub_client_service.dart';
import 'package:collection/collection.dart';
import 'package:flutter/foundation.dart';
import 'package:freezed_annotation/freezed_annotation.dart';

part 'playlist_bloc.freezed.dart';
part 'playlist_event.dart';
part 'playlist_state.dart';

class PlayListBloc extends Bloc<PlayListEvent, PlayListState> {
  final CastItHubClientService _castItHub;
  final List<StreamSubscription> _subscriptions = [];

  PlayListState get initialState => const PlayListState.loading();

  PlayListStateLoadedState get currentState => state as PlayListStateLoadedState;

  PlayListBloc(this._castItHub) : super(const PlayListState.loading()) {
    on<PlayListEventDisconnected>((event, emit) => PlayListState.disconnected(playListId: event.playListId));

    on<PlayListEventLoad>((event, emit) async {
      emit(initialState);
      final playList = await _castItHub.loadPlayList(event.id);
      final updatedState = PlayListState.loaded(
        playlistId: playList.id,
        name: playList.name,
        loop: playList.loop,
        position: playList.position,
        shuffle: playList.shuffle,
        files: playList.files,
        loaded: true,
        scrollToFileId: event.scrollToFileId,
      );
      emit(updatedState);
      _clearPreviousScrolledFileIfNeeded(emit);
    });

    on<PlayListEventLoaded>((event, emit) {
      emit(initialState);
      final updatedState = PlayListState.loaded(
        playlistId: event.playlist.id,
        name: event.playlist.name,
        loop: event.playlist.loop,
        position: event.playlist.position,
        shuffle: event.playlist.shuffle,
        files: event.playlist.files,
        loaded: true,
      );
      emit(updatedState);
      _clearPreviousScrolledFileIfNeeded(emit);
    });

    on<PlayListEventPlayListOptionsChanged>(
      (event, emit) => emit(currentState.copyWith(loop: event.loop, shuffle: event.shuffle)),
    );

    on<PlayListEventToggleSearchBoxVisibility>(
      (event, emit) => emit(currentState.copyWith(searchBoxIsVisible: !currentState.searchBoxIsVisible)),
    );

    on<PlayListEventSearchBoxTextChanged>((event, emit) {
      final isFiltering = !event.text.isNullEmptyOrWhitespace;
      final filteredFiles =
          !isFiltering
              ? <FileItemResponseDto>[]
              : currentState.files.where((element) => element.filename.toLowerCase().contains(event.text.toLowerCase())).toList();

      final updatedState = currentState.copyWith(filteredFiles: filteredFiles, isFiltering: isFiltering);
      emit(updatedState);
    });

    on<PlayListEventClosePage>((event, emit) => emit(const PlayListState.close()));

    on<PlayListEventPlayListNotFound>((event, emit) => emit(const PlayListState.notFound()));

    on<PlayListEventPlayListChanged>((event, emit) => emit(_handlePlayListChanged(event.playList)));

    on<PlayListEventPlayListDeleted>((event, emit) => emit(_handlePlayListDeleted(event.id)));

    on<PlayListEventFileAdded>((event, emit) => emit(_handleFileAdded(event.file)));

    on<PlayListEventFilesChanged>((event, emit) => emit(_handleFilesChanged(event.files)));

    on<PlayListEventFileChanged>((event, emit) => emit(_handleFileChanged(event.file)));

    on<PlayListEventFilesDeleted>((event, emit) => emit(_handleFileDeleted(event.playListId, event.id)));
  }

  void _clearPreviousScrolledFileIfNeeded(Emitter<PlayListState> emit) {
    //Clear any previous scrolled file
    if (state is PlayListStateLoadedState && currentState.scrollToFileId != null) {
      final updatedState = currentState.copyWith.call(scrollToFileId: null);
      emit(updatedState);
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
    switch (state) {
      case final PlayListStateLoadedState state:
        if (state.playlistId != playList.id) {
          return state;
        }
        return state.copyWith.call(
          shuffle: playList.shuffle,
          name: playList.name,
          position: playList.position,
          loop: playList.loop,
        );
      default:
        return state;
    }
  }

  PlayListState _handlePlayListDeleted(int id) {
    switch (state) {
      case final PlayListStateLoadedState state:
        if (state.playlistId != id) {
          return state;
        }

        return const PlayListState.close();
      default:
        return state;
    }
  }

  PlayListState _handleFileAdded(FileItemResponseDto file) {
    switch (state) {
      case final PlayListStateLoadedState state:
        if (state.playlistId != file.playListId) {
          return state;
        }

        final files = [...state.files];
        files.insert(file.position - 1, file);

        return state.copyWith.call(files: files);
      default:
        return state;
    }
  }

  PlayListState _handleFilesChanged(List<FileItemResponseDto> files) {
    return switch (state) {
      final PlayListStateLoadedState state => state.copyWith(files: files),
      _ => state,
    };
  }

  PlayListState _handleFileChanged(FileItemResponseDto file) {
    switch (state) {
      case final PlayListStateLoadedState state:
        if (state.playlistId != file.playListId) {
          return state;
        }

        final files = [...state.files];
        final current = files.firstWhereOrNull((el) => el.id == file.id);
        if (current == null || file == current) {
          return state;
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

        return state.copyWith(files: files);
      default:
        return state;
    }
  }

  PlayListState _handleFileDeleted(int playListId, int id) {
    switch (state) {
      case final PlayListStateLoadedState state:
        if (state.playlistId != playListId) {
          return state;
        }

        final files = [...state.files];
        files.removeWhere((el) => el.id == id);
        return state.copyWith(files: files);
      default:
        return state;
    }
  }

  void _onPlayListLoaded(PlayListItemResponseDto? event) {
    //Playlist does not exist
    if (event == null) {
      add(const PlayListEvent.notFound());
      return;
    }

    if (state is! PlayListStateLoadedState || currentState.playlistId == event.id) {
      add(PlayListEvent.loaded(playlist: event));
      return;
    }
  }

  void _onConnected() {
    switch (state) {
      case final PlayListStateDisconnectedState state:
        if (state.playListId != null) {
          add(PlayListEvent.load(id: state.playListId!));
        }
      default:
        break;
    }
  }

  void _onDisconnected() {
    switch (state) {
      case final PlayListStateLoadedState state:
        add(PlayListEvent.disconnected(playListId: state.playlistId));
      default:
        break;
    }
  }

  void _onRefresh(RefreshPlayListResponseDto event) {
    if (state is! PlayListStateLoadedState || currentState.playlistId != event.id) {
      return;
    }
    if (!event.wasDeleted) {
      add(PlayListEvent.load(id: event.id));
    } else {
      add(const PlayListEvent.closePage());
    }
  }

  void _onPlayListsChanged(List<GetAllPlayListResponseDto> event) {
    switch (state) {
      case final PlayListStateLoadedState state:
        final playList = event.firstWhereOrNull((el) => el.id == state.playlistId);
        if (playList == null) {
          return;
        }

        add(PlayListEvent.playListChanged(playList: playList));
      default:
        break;
    }
  }

  void _onPlayListChanged((bool, GetAllPlayListResponseDto) event) {
    switch (state) {
      case PlayListStateLoadedState():
        final changeComesFromPlayedFile = event.$1;
        final playList = event.$2;
        if (!changeComesFromPlayedFile) {
          add(PlayListEvent.playListChanged(playList: playList));
        }
      default:
        break;
    }
  }

  void _onPlayListDeleted(int event) {
    switch (state) {
      case PlayListStateLoadedState():
        add(PlayListEvent.playListDeleted(id: event));
      default:
        break;
    }
  }

  void _onFileAdded(FileItemResponseDto event) {
    switch (state) {
      case PlayListStateLoadedState():
        add(PlayListEvent.fileAdded(file: event));
      default:
        break;
    }
  }

  void _onFileChanged((bool, FileItemResponseDto) event) {
    switch (state) {
      case final PlayListStateLoadedState state:
        //The second part of the if is to make sure that we only trigger the change
        // if we had a previously played file
        final changeComesFromPlayedFile = event.$1;
        final file = event.$2;
        final currentPlayedFile = state.files.firstWhereOrNull((el) => el.isBeingPlayed);
        if (!changeComesFromPlayedFile || currentPlayedFile?.id != file.id) {
          add(PlayListEvent.fileChanged(file: file));
        }
      default:
        break;
    }
  }

  void _onFilesChanged(List<FileItemResponseDto> event) {
    switch (state) {
      case PlayListStateLoadedState():
        add(PlayListEvent.filesChanged(files: event));
      default:
        break;
    }
  }

  void _onFileDeleted((int, int) event) {
    switch (state) {
      case PlayListStateLoadedState():
        add(PlayListEvent.fileDeleted(playListId: event.$1, id: event.$2));
      default:
        break;
    }
  }
}
