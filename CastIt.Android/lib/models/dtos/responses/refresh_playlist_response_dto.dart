import 'package:freezed_annotation/freezed_annotation.dart';

part 'refresh_playlist_response_dto.freezed.dart';
part 'refresh_playlist_response_dto.g.dart';

@freezed
abstract class RefreshPlayListResponseDto implements _$RefreshPlayListResponseDto {
  factory RefreshPlayListResponseDto({
    @JsonKey(name: 'Id') required int id,
    @JsonKey(name: 'WasDeleted') required bool wasDeleted,
  }) = _RefreshPlayListResponseDto;

  factory RefreshPlayListResponseDto.fromJson(Map<String, dynamic> json) => _$RefreshPlayListResponseDtoFromJson(json);

  static List<String> get jsonKeys => ['Id', 'WasDeleted'];
}
