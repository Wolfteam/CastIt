part of 'playlists_bloc.dart';

@freezed
sealed class PlayListsEvent with _$PlayListsEvent {
  const factory PlayListsEvent.load() = PlayListsEventLoad;

  const factory PlayListsEvent.loaded({required List<GetAllPlayListResponseDto> playlists}) = PlayListsEventLoaded;

  const factory PlayListsEvent.added({required GetAllPlayListResponseDto playList}) = PlayListsEventAdded;

  const factory PlayListsEvent.changed({required GetAllPlayListResponseDto playList}) = PlayListsEventChanged;

  const factory PlayListsEvent.deleted({required int id}) = PlayListsEventDeleted;

  const factory PlayListsEvent.disconnected() = PlayListsEventDisconnected;
}
