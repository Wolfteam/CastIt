import 'package:castit/models/dtos/dtos.dart';
import 'package:freezed_annotation/freezed_annotation.dart';

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
      loopPlayList: status.playList!.loop,
      playListId: status.playList!.id,
      playListName: status.playList!.name,
      shufflePlayList: status.playList!.shuffle,
      thumbnailUrl: status.playedFile!.thumbnailUrl,
    );
  }
//
// static List<String> get jsonKeys => [
//       'Id',
//       'Filename',
//       'ThumbnailUrl',
//       'Duration',
//       'LoopFile',
//       'CurrentSeconds',
//       'IsPaused',
//       'VolumeLevel',
//       'IsMuted',
//       'PlayListId',
//       'PlayListName',
//       'LoopPlayList',
//       'ShufflePlayList'
//     ];
}
