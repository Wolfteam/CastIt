import 'package:castit/domain/models/models.dart';
import 'package:freezed_annotation/freezed_annotation.dart';

part 'playlist_item_response_dto.freezed.dart';
part 'playlist_item_response_dto.g.dart';

@freezed
sealed class PlayListItemResponseDto with _$PlayListItemResponseDto {
  const factory PlayListItemResponseDto({
    required int id,
    required String name,
    required int position,
    required bool loop,
    required bool shuffle,
    required int numberOfFiles,
    required String playedTime,
    required String totalDuration,
    required String imageUrl,
    required List<FileItemResponseDto> files,
  }) = _PlayListItemResponseDto;

  factory PlayListItemResponseDto.fromJson(Map<String, dynamic> json) => _$PlayListItemResponseDtoFromJson(json);
}
