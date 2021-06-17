using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Interfaces;
using System.Collections.Generic;

namespace CastIt.Application.Server
{
    public delegate void OnAppClosingHandler();
    public delegate void OnAppSettingsChangedHandler();
    public delegate void OnCastDevicesChangedHandler(List<IReceiver> devices);

    public delegate void OnPlayListAddedHandler(GetAllPlayListResponseDto playList);
    public delegate void OnPlayListChangedHandler(GetAllPlayListResponseDto playList);
    public delegate void OnPlayListDeletedHandler(long id);

    public delegate void OnFileAddedHandler(FileItemResponseDto file);
    public delegate void OnFileChangedHandler(FileItemResponseDto file);
    public delegate void OnFileDeletedHandler(long playlistId, long id);
}
