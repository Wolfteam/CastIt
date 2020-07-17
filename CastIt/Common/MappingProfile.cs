using AutoMapper;
using CastIt.GoogleCast.Interfaces;
using CastIt.Models.Entities;
using CastIt.Server.Dtos.Responses;
using CastIt.ViewModels.Items;

namespace CastIt.Common
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<FileItem, FileItemViewModel>()
                .ConstructUsingServiceLocator();
            CreateMap<PlayList, PlayListItemViewModel>()
                .ConstructUsingServiceLocator();
            CreateMap<IReceiver, DeviceItemViewModel>()
                .ConstructUsingServiceLocator();

            CreateMap<FileItem, FileItemResponseDto>();
            CreateMap<PlayList, GetAllPlayListResponseDto>();
            CreateMap<PlayList, PlayListItemResponseDto>();
            CreateMap<FileItemOptionsViewModel, FileItemOptionsResponseDto>();
        }
    }
}
