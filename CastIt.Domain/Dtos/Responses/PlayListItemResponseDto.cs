using System.Collections.Generic;

namespace CastIt.Domain.Dtos.Responses
{
    public class PlayListItemResponseDto : GetAllPlayListResponseDto
    {
        public List<FileItemResponseDto> Files { get; set; }
    }
}
