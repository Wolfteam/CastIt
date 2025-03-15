import 'package:freezed_annotation/freezed_annotation.dart';

part 'file_item_options_response_dto.freezed.dart';
part 'file_item_options_response_dto.g.dart';

@freezed
sealed class FileItemOptionsResponseDto with _$FileItemOptionsResponseDto {
  factory FileItemOptionsResponseDto({
    required int id,
    required bool isVideo,
    required bool isAudio,
    required bool isSubTitle,
    required bool isQuality,
    required String text,
    required bool isSelected,
    required bool isEnabled,
    String? path,
  }) = _FileItemOptionsResponseDto;

  factory FileItemOptionsResponseDto.fromJson(Map<String, dynamic> json) => _$FileItemOptionsResponseDtoFromJson(json);
}
