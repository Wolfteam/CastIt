import 'package:freezed_annotation/freezed_annotation.dart';
import 'package:json_annotation/json_annotation.dart';

part 'file_item_options_response_dto.freezed.dart';
part 'file_item_options_response_dto.g.dart';

@freezed
abstract class FileItemOptionsResponseDto implements _$FileItemOptionsResponseDto {
  factory FileItemOptionsResponseDto({
    @JsonKey(name: 'Id') required int id,
    @JsonKey(name: 'IsVideo') required bool isVideo,
    @JsonKey(name: 'IsAudio') required bool isAudio,
    @JsonKey(name: 'IsSubTitle') required bool isSubTitle,
    @JsonKey(name: 'IsQuality') required bool isQuality,
    @JsonKey(name: 'Text') required String text,
    @JsonKey(name: 'IsSelected') required bool isSelected,
    @JsonKey(name: 'IsEnabled') required bool isEnabled,
    @JsonKey(name: 'Path') String? path,
  }) = _FileItemOptionsResponseDto;

  factory FileItemOptionsResponseDto.fromJson(Map<String, dynamic> json) => _$FileItemOptionsResponseDtoFromJson(json);

  static List<String> get jsonKeys => [
        'Id',
        'IsVideo',
        'IsAudio',
        'IsSubTitle',
        'IsQuality',
        'Path',
        'Text',
        'IsSelected',
        'IsEnabled',
      ];
}
