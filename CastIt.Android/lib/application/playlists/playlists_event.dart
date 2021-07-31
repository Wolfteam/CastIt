part of 'playlists_bloc.dart';

@freezed
class PlayListsEvent with _$PlayListsEvent {
  factory PlayListsEvent.load() = _Load;

  factory PlayListsEvent.loaded({
    required List<GetAllPlayListResponseDto> playlists,
  }) = _Loaded;

  factory PlayListsEvent.added({
    required GetAllPlayListResponseDto playList,
  }) = _Added;

  factory PlayListsEvent.changed({
    required GetAllPlayListResponseDto playList,
  }) = _Changed;

  factory PlayListsEvent.deleted({
    required int id,
  }) = _Deleted;

  factory PlayListsEvent.disconnected() = _Disconnected;
}
