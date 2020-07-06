part of 'play_bloc.dart';

@freezed
abstract class PlayState implements _$PlayState {
  factory PlayState.connecting() = ConnectingState;
  factory PlayState.connected() = ConnectedState;
  factory PlayState.fileLoading() = FileLoadingState;
  factory PlayState.fileLoadingFailed({
    @required String msg,
  }) = FileLoadingFailedState;
  factory PlayState.playing({
    int id,
    int playListId,
    String filename,
    String playlistName,
    bool loopFile,
    bool shufflePlayList,
    double duration,
    double currentSeconds,
    bool isPaused,
    String thumbPath,
  }) = PlayingState;
}
