using System.Collections.Generic;

namespace CastIt.Domain.Dtos
{
    public class AppListResponseDto<T> : AppResponseDto<List<T>>
    {
        public AppListResponseDto()
        {
            Result = new List<T>();
        }

        public AppListResponseDto(bool succeed, List<T> result) 
            : base(result, succeed)
        {
        }
    }
}
