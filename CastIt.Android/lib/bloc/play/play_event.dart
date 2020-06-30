part of 'play_bloc.dart';

@freezed
abstract class PlayEvent implements _$PlayEvent {
  factory PlayEvent.connected() = Connected;

  factory PlayEvent.fileLoading() = FileLoading;

  factory PlayEvent.fileLoadingError({@required String msg}) = FileLoadingError;

  factory PlayEvent.fileLoaded({
    @required String filename,
    String thumbPath,
    @required double duration,
    @required bool loopFile,
    @required double currentSeconds,
    @required bool isPaused,
    @required double volumeLevel,
    @required bool isMuted,
    String playlistName,
    bool shufflePlayList,
  }) = Playing;

  factory PlayEvent.timeChanged({
    @required double seconds,
  }) = TimeChanged;

  factory PlayEvent.paused() = Paused;

  factory PlayEvent.stopped() = Stopped;

  factory PlayEvent.volumeLvlChanged({
    @required double newLvl,
    @required bool isMuted,
  }) = VolumeLevelChanged;

  factory PlayEvent.disconnected() = Disconnected;
}
