using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Entities;
using CastIt.GoogleCast;
using CastIt.Shared.Models;
using Mapster;

namespace CastIt.Server.Common
{
    public static class MappingConfig
    {
        public static void RegisterMappings(this TypeAdapterConfig config)
        {
            config.NewConfig<FileItem, FileItemResponseDto>();
            config.NewConfig<PlayList, GetAllPlayListResponseDto>();
            config.NewConfig<PlayList, PlayListItemResponseDto>();
            config.NewConfig<PlayList, ServerPlayList>();
            config.NewConfig<ServerPlayList, GetAllPlayListResponseDto>();
            config.NewConfig<ServerPlayList, PlayListItemResponseDto>();
            config.NewConfig<ServerFileItem, FileItemResponseDto>();
            config.NewConfig<PlayerStatus, PlayerStatusResponseDto>();
        }
    }
}
