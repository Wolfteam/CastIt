import 'package:castit/domain/models/models.dart';
import 'package:freezed_annotation/freezed_annotation.dart';

part 'server_player_status_response_dto.freezed.dart';
part 'server_player_status_response_dto.g.dart';

@freezed
class ServerPlayerStatusResponseDto with _$ServerPlayerStatusResponseDto {
  const factory ServerPlayerStatusResponseDto({
    required PlayerStatusResponseDto player,
    GetAllPlayListResponseDto? playList,
    FileItemResponseDto? playedFile,
  }) = _ServerPlayerStatusResponseDto;

  factory ServerPlayerStatusResponseDto.fromJson(Map<String, dynamic> json) => _$ServerPlayerStatusResponseDtoFromJson(json);
}
