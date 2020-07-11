import 'package:json_annotation/json_annotation.dart';

import '../../models/dtos/dtos.dart';

class JsonGenericConverter<T> implements JsonConverter<T, Object> {
  const JsonGenericConverter();

  @override
  T fromJson(Object json) {
    if (json is Map<String, dynamic> && _allKeysArePresent(FileItemResponseDto.jsonKeys, json.keys)) {
      return FileItemResponseDto.fromJson(json) as T;
    }

    if (json is Map<String, dynamic> && _allKeysArePresent(PlayListItemResponseDto.jsonKeys, json.keys)) {
      return PlayListItemResponseDto.fromJson(json) as T;
    }

    if (json is Map<String, dynamic> && _allKeysArePresent(GetAllPlayListResponseDto.jsonKeys, json.keys)) {
      return GetAllPlayListResponseDto.fromJson(json) as T;
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

    if (json is Map<String, dynamic> && _allKeysArePresent(FileItemOptionsResponseDto.jsonKeys, json.keys)) {
      return FileItemOptionsResponseDto.fromJson(json) as T;
    }

    if (json is Map<String, dynamic> && _allKeysArePresent(RefreshPlayListResponseDto.jsonKeys, json.keys)) {
      return RefreshPlayListResponseDto.fromJson(json) as T;
    }

    if (json is List<dynamic>) {
      final items = json.map(fromJson).toList();

      return items as T;
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
