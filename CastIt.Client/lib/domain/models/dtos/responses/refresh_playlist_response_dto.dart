import 'package:freezed_annotation/freezed_annotation.dart';

part 'refresh_playlist_response_dto.freezed.dart';
part 'refresh_playlist_response_dto.g.dart';

@freezed
class RefreshPlayListResponseDto with _$RefreshPlayListResponseDto {
  factory RefreshPlayListResponseDto({
    required int id,
    required bool wasDeleted,
  }) = _RefreshPlayListResponseDto;

  factory RefreshPlayListResponseDto.fromJson(Map<String, dynamic> json) => _$RefreshPlayListResponseDtoFromJson(json);
}
