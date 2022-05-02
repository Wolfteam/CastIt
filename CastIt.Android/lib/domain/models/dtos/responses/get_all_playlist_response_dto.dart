import 'package:freezed_annotation/freezed_annotation.dart';

part 'get_all_playlist_response_dto.freezed.dart';
part 'get_all_playlist_response_dto.g.dart';

@freezed
class GetAllPlayListResponseDto with _$GetAllPlayListResponseDto {
  const factory GetAllPlayListResponseDto({
    required int id,
    required String name,
    required int position,
    required bool loop,
    required bool shuffle,
    required int numberOfFiles,
    required String playedTime,
    required String totalDuration,
    String? imageUrl,
  }) = _GetAllPlayListResponseDto;

  factory GetAllPlayListResponseDto.fromJson(Map<String, dynamic> json) => _$GetAllPlayListResponseDtoFromJson(json);
}
