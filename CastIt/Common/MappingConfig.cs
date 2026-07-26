using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Entities;
using CastIt.ViewModels.Items;
using Mapster;

namespace CastIt.Common
{
    public static class MappingConfig
    {
        public static void RegisterMappings(this TypeAdapterConfig config)
        {
            config.NewConfig<FileItemViewModel, FileItemResponseDto>();
            config.NewConfig<PlayList, GetAllPlayListResponseDto>();
            config.NewConfig<PlayList, PlayListItemResponseDto>();
            config.NewConfig<FileItemOptionsViewModel, FileItemOptionsResponseDto>();
            config.NewConfig<FileItemOptionsResponseDto, FileItemOptionsViewModel>();
            config.NewConfig<FileItemResponseDto, FileItemViewModel>();
            config.NewConfig<GetAllPlayListResponseDto, PlayListItemViewModel>();
        }
    }
}
