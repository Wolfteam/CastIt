import 'package:freezed_annotation/freezed_annotation.dart';

import 'models.dart';

part 'played_file.freezed.dart';

@freezed
class PlayedFile with _$PlayedFile {
  factory PlayedFile({
    required int id,
    required String filename,
    required double duration,
    required bool loopFile,
    required double currentSeconds,
    required bool isPaused,
    required double volumeLevel,
    required bool isMuted,
    required int playListId,
    required String playListName,
    required bool loopPlayList,
    required bool shufflePlayList,
    String? thumbnailUrl,
    String? playListPlayedTime,
    String? playListTotalDuration,
  }) = _PlayedFile;

  // ignore: prefer_constructors_over_static_methods
  static PlayedFile from(ServerPlayerStatusResponseDto status) {
    return PlayedFile(
      volumeLevel: status.player.volumeLevel,
      isMuted: status.player.isMuted,
      currentSeconds: status.player.elapsedSeconds,
      duration: status.player.currentMediaDuration,
      filename: status.playedFile!.filename,
      id: status.playedFile!.id,
      isPaused: status.player.isPaused,
      loopFile: status.playedFile!.loop,
      loopPlayList: status.playList?.loop ?? false,
      playListId: status.playList?.id ?? -1,
      playListName: status.playList?.name ?? 'N/A',
      shufflePlayList: status.playList?.shuffle ?? false,
      thumbnailUrl: status.playedFile!.thumbnailUrl,
      playListPlayedTime: status.playList?.playedTime,
      playListTotalDuration: status.playList?.totalDuration,
    );
  }
}
