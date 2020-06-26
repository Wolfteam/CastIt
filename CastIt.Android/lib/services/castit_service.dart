import 'package:flutter/widgets.dart';

import '../models/dtos/app_list_response_dto.dart';
import '../models/dtos/responses/file_response_dto.dart';
import '../models/dtos/responses/playlist_response_dto.dart';
import 'api/castit_api.dart';

abstract class CastItService {
  Future<AppListResponseDto<PlayListResponseDto>> getAllPlayLists();

  Future<AppListResponseDto<FileResponseDto>> getAllFiles(int playListId);
}

class CastItServiceImpl extends CastItService {
  final CastItApi api;
  CastItServiceImpl({
    @required this.api,
  });

  @override
  Future<AppListResponseDto<FileResponseDto>> getAllFiles(
    int playListId,
  ) async {
    var response = AppListResponseDto<FileResponseDto>();
    try {
      final resp = await api.getAllFiles(playListId);
      if (resp.succeed) {
        response = resp;
      }
    } catch (e) {
      print(e);
    }

    return response;
  }

  @override
  Future<AppListResponseDto<PlayListResponseDto>> getAllPlayLists() async {
    var response = AppListResponseDto<PlayListResponseDto>();
    try {
      final resp = await api.getAllPlayLists();
      if (resp.succeed) {
        response = resp;
      }
    } catch (e) {
      print(e);
    }

    return response;
  }
}
