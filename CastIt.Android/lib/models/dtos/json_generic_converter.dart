import 'package:json_annotation/json_annotation.dart';

import 'responses/file_response_dto.dart';
import 'responses/playlist_response_dto.dart';

class JsonGenericConverter<T> implements JsonConverter<T, Object> {
  const JsonGenericConverter();

  @override
  T fromJson(Object json) {
    if (json is Map<String, dynamic> &&
        json.containsKey('Id') &&
        json.containsKey('PlayListId')) {
          return FileResponseDto.fromJson(json) as T;
    }
    if (json is Map<String, dynamic> &&
        json.containsKey('Loop')) {
      return PlayListResponseDto.fromJson(json) as T;
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
}
