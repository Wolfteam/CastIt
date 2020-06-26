// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'castit_api.dart';

// **************************************************************************
// RetrofitGenerator
// **************************************************************************

class _CastItApi implements CastItApi {
  _CastItApi(this._dio, {this.baseUrl}) {
    ArgumentError.checkNotNull(_dio, '_dio');
  }

  final Dio _dio;

  String baseUrl;

  @override
  getAllPlayLists() async {
    const _extra = <String, dynamic>{};
    final queryParameters = <String, dynamic>{};
    final _data = <String, dynamic>{};
    final Response<Map<String, dynamic>> _result = await _dio.request(
        '/playlists',
        queryParameters: queryParameters,
        options: RequestOptions(
            method: 'GET',
            headers: <String, dynamic>{},
            extra: _extra,
            baseUrl: baseUrl),
        data: _data);
    final value =
        AppListResponseDto<PlayListResponseDto>.fromJson(_result.data);
    return value;
  }

  @override
  getAllFiles(playlistId) async {
    ArgumentError.checkNotNull(playlistId, 'playlistId');
    const _extra = <String, dynamic>{};
    final queryParameters = <String, dynamic>{};
    final _data = <String, dynamic>{};
    final Response<Map<String, dynamic>> _result = await _dio.request(
        '/playlists/$playlistId/files',
        queryParameters: queryParameters,
        options: RequestOptions(
            method: 'GET',
            headers: <String, dynamic>{},
            extra: _extra,
            baseUrl: baseUrl),
        data: _data);
    final value = AppListResponseDto<FileResponseDto>.fromJson(_result.data);
    return value;
  }
}
