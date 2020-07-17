using System.Collections.Generic;

namespace CastIt.Server.Dtos.Responses
{
    public class PlayListItemResponseDto : GetAllPlayListResponseDto
    {
        public List<FileItemResponseDto> Files { get; set; }
    }
}
