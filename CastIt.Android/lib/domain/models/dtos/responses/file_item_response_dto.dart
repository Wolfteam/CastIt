import 'package:freezed_annotation/freezed_annotation.dart';
import 'package:json_annotation/json_annotation.dart';

import '../../models.dart';

part 'file_item_response_dto.freezed.dart';
part 'file_item_response_dto.g.dart';

@freezed
class FileItemResponseDto with _$FileItemResponseDto {
  List<FileItemOptionsResponseDto> get streams => currentFileVideos + currentFileAudios + currentFileSubTitles + currentFileQualities;

  factory FileItemResponseDto({
    required int id,
    String? name,
    String? description,
    required double totalSeconds,
    required String path,
    required int position,
    required double playedPercentage,
    required int playListId,
    required bool loop,
    required bool isBeingPlayed,
    required bool isLocalFile,
    required bool isUrlFile,
    required double playedSeconds,
    required bool canStartPlayingFromCurrentPercentage,
    required bool wasPlayed,
    required bool isCached,
    required bool exists,
    required String filename,
    String? size,
    String? extension,
    required String subTitle,
    required String playedTime,
    required String totalDuration,
    required String fullTotalDuration,
    required List<FileItemOptionsResponseDto> currentFileVideos,
    required List<FileItemOptionsResponseDto> currentFileAudios,
    required List<FileItemOptionsResponseDto> currentFileSubTitles,
    required List<FileItemOptionsResponseDto> currentFileQualities,
    required int currentFileVideoStreamIndex,
    required int currentFileAudioStreamIndex,
    required int currentFileSubTitleStreamIndex,
    required int currentFileQuality,
    String? thumbnailUrl,
  }) = _FileItemResponseDto;

  FileItemResponseDto._();

  factory FileItemResponseDto.fromJson(Map<String, dynamic> json) => _$FileItemResponseDtoFromJson(json);
}
