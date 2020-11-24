using AutoMapper;
using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Entities;
using CastIt.Domain.Interfaces;
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

            CreateMap<FileItemViewModel, FileItemResponseDto>();
            CreateMap<PlayList, GetAllPlayListResponseDto>();
            CreateMap<PlayList, PlayListItemResponseDto>();
            CreateMap<FileItemOptionsViewModel, FileItemOptionsResponseDto>();
        }
    }
}
