import 'package:freezed_annotation/freezed_annotation.dart';

part 'file_loaded_response_dto.freezed.dart';
part 'file_loaded_response_dto.g.dart';

@freezed
abstract class FileLoadedResponseDto implements _$FileLoadedResponseDto {
  factory FileLoadedResponseDto({
    @required @JsonKey(name: 'Filename') String filename,
    @required @JsonKey(name: 'Duration') double duration,
    @required @JsonKey(name: 'LoopFile') bool loopFile,
    @required @JsonKey(name: 'CurrentSeconds') double currentSeconds,
    @required @JsonKey(name: 'IsPaused') bool isPaused,
    @required @JsonKey(name: 'VolumeLevel') double volumeLevel,
    @required @JsonKey(name: 'IsMuted') bool isMuted,
    @JsonKey(name: 'PlayListName') String playListName,
    @JsonKey(name: 'ShufflePlayList') bool shufflePlayList,
    @JsonKey(name: 'ThumbnailUrl') String thumbnailUrl,
  }) = _FileLoadedResponseDto;

  factory FileLoadedResponseDto.fromJson(Map<String, dynamic> json) => _$FileLoadedResponseDtoFromJson(json);

  static List<String> get jsonKeys => [
        'Filename',
        'ThumbnailUrl',
        'Duration',
        'LoopFile',
        'CurrentSeconds',
        'IsPaused',
        'VolumeLevel',
        'IsMuted',
        'PlayListName',
        'ShufflePlayList'
      ];
}
