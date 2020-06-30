import 'package:json_annotation/json_annotation.dart';

import '../../models/dtos/responses/app_settings_response_dto.dart';
import '../../models/dtos/responses/file_loaded_response_dto.dart';
import '../../models/dtos/responses/file_response_dto.dart';
import '../../models/dtos/responses/playlist_response_dto.dart';
import '../../models/dtos/responses/volume_level_changed_response_dto.dart';

class JsonGenericConverter<T> implements JsonConverter<T, Object> {
  const JsonGenericConverter();

  @override
  T fromJson(Object json) {
    if (json is Map<String, dynamic> && _allKeysArePresent(FileResponseDto.jsonKeys, json.keys)) {
      return FileResponseDto.fromJson(json) as T;
    }
    if (json is Map<String, dynamic> && _allKeysArePresent(PlayListResponseDto.jsonKeys, json.keys)) {
      return PlayListResponseDto.fromJson(json) as T;
    }

    if (json is Map<String, dynamic> && _allKeysArePresent(FileLoadedResponseDto.jsonKeys, json.keys)) {
      return FileLoadedResponseDto.fromJson(json) as T;
    }

    if (json is Map<String, dynamic> && _allKeysArePresent(VolumeLevelChangedResponseDto.jsonKeys, json.keys)) {
      return VolumeLevelChangedResponseDto.fromJson(json) as T;
    }

    if (json is Map<String, dynamic> && _allKeysArePresent(AppSettingsResponseDto.jsonKeys, json.keys)) {
      return AppSettingsResponseDto.fromJson(json) as T;
    }

    // This will only work if `json` is a native JSON type:
    //   num, String, bool, null, etc
    // *and* is assignable to `T`.
    return json as T;
  }

  @override
  Object toJson(T object) {
    // This will only work if `object` is a native JSON type:
    //   num, String, bool, null, etc
    // Or if it has a `toJson()` function`.
    return object;
  }

  bool _allKeysArePresent(List<String> parent, Iterable<String> jsonKeys) =>
      parent.every((element) => jsonKeys.contains(element));
}
