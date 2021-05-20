import 'package:freezed_annotation/freezed_annotation.dart';
import 'package:json_annotation/json_annotation.dart';

part 'get_all_playlist_response_dto.freezed.dart';
part 'get_all_playlist_response_dto.g.dart';

@freezed
abstract class GetAllPlayListResponseDto implements _$GetAllPlayListResponseDto {
  const factory GetAllPlayListResponseDto({
    @JsonKey(name: 'Id') required int id,
    @JsonKey(name: 'Name') required String name,
    @JsonKey(name: 'Position') required int position,
    @JsonKey(name: 'Loop') required bool loop,
    @JsonKey(name: 'Shuffle') required bool shuffle,
    @JsonKey(name: 'NumberOfFiles') required int numberOfFiles,
    @JsonKey(name: 'TotalDuration') required String totalDuration,
  }) = _GetAllPlayListResponseDto;

  factory GetAllPlayListResponseDto.fromJson(Map<String, dynamic> json) => _$GetAllPlayListResponseDtoFromJson(json);

  static List<String> get jsonKeys => [
        'Id',
        'Name',
        'Position',
        'Loop',
        'Shuffle',
        'NumberOfFiles',
        'TotalDuration',
      ];
}
