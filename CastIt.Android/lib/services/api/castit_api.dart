import 'package:dio/dio.dart';
import 'package:retrofit/http.dart';

import '../../models/dtos/app_list_response_dto.dart';
import '../../models/dtos/responses/file_response_dto.dart';
import '../../models/dtos/responses/playlist_response_dto.dart';

part 'castit_api.g.dart';

@RestApi()
abstract class CastItApi {
  static _CastItApi instance;

  factory CastItApi(Dio dio, {String baseUrl}) {
    final api = _CastItApi(dio, baseUrl: baseUrl);
    CastItApi.instance = api;
    return api;
  }

  @GET('/playlists')
  Future<AppListResponseDto<PlayListResponseDto>> getAllPlayLists();

  @GET('/playlists/{playlistId}/files')
  Future<AppListResponseDto<FileResponseDto>> getAllFiles(
    @Path('playlistId') int playlistId,
  );
}
