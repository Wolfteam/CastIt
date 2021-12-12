using AutoMapper;
using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Entities;
using CastIt.GoogleCast;
using CastIt.Shared.Models;

namespace CastIt.Server.Common
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<FileItem, FileItemResponseDto>();
            CreateMap<PlayList, GetAllPlayListResponseDto>();
            CreateMap<PlayList, PlayListItemResponseDto>();

            CreateMap<PlayList, ServerPlayList>();
            CreateMap<ServerPlayList, GetAllPlayListResponseDto>();
            CreateMap<ServerPlayList, PlayListItemResponseDto>();
            CreateMap<ServerFileItem, FileItemResponseDto>();

            CreateMap<PlayerStatus, PlayerStatusResponseDto>();
        }
    }
}
