import 'package:freezed_annotation/freezed_annotation.dart';

part 'file_loaded_response_dto.freezed.dart';
part 'file_loaded_response_dto.g.dart';

@freezed
abstract class FileLoadedResponseDto implements _$FileLoadedResponseDto {
  factory FileLoadedResponseDto({
    @JsonKey(name: 'Id') required int id,
    @JsonKey(name: 'Filename') required String filename,
    @JsonKey(name: 'Duration') required double duration,
    @JsonKey(name: 'LoopFile') required bool loopFile,
    @JsonKey(name: 'CurrentSeconds') required double currentSeconds,
    @JsonKey(name: 'IsPaused') required bool isPaused,
    @JsonKey(name: 'VolumeLevel') required double volumeLevel,
    @JsonKey(name: 'IsMuted') required bool isMuted,
    @JsonKey(name: 'PlayListId') required int playListId,
    @JsonKey(name: 'PlayListName') required String playListName,
    @JsonKey(name: 'LoopPlayList') required bool loopPlayList,
    @JsonKey(name: 'ShufflePlayList') required bool shufflePlayList,
    @JsonKey(name: 'ThumbnailUrl') String? thumbnailUrl,
  }) = _FileLoadedResponseDto;

  factory FileLoadedResponseDto.fromJson(Map<String, dynamic> json) => _$FileLoadedResponseDtoFromJson(json);

  static List<String> get jsonKeys => [
        'Id',
        'Filename',
        'ThumbnailUrl',
        'Duration',
        'LoopFile',
        'CurrentSeconds',
        'IsPaused',
        'VolumeLevel',
        'IsMuted',
        'PlayListId',
        'PlayListName',
        'LoopPlayList',
        'ShufflePlayList'
      ];
}
