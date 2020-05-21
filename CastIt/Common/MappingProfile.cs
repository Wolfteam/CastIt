using AutoMapper;
using CastIt.Models.Entities;
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
        }
    }
}
