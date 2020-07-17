import 'package:freezed_annotation/freezed_annotation.dart';
import 'package:json_annotation/json_annotation.dart';

part 'file_item_options_response_dto.freezed.dart';
part 'file_item_options_response_dto.g.dart';

@freezed
abstract class FileItemOptionsResponseDto implements _$FileItemOptionsResponseDto {
  factory FileItemOptionsResponseDto({
    @required @JsonKey(name: 'Id') int id,
    @required @JsonKey(name: 'IsVideo') bool isVideo,
    @required @JsonKey(name: 'IsAudio') bool isAudio,
    @required @JsonKey(name: 'IsSubTitle') bool isSubTitle,
    @required @JsonKey(name: 'IsQuality') bool isQuality,
    @required @JsonKey(name: 'Text') String text,
    @required @JsonKey(name: 'IsSelected') bool isSelected,
    @required @JsonKey(name: 'IsEnabled') bool isEnabled,
    @JsonKey(name: 'Path') String path,
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
