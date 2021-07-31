using System.Collections.Generic;

namespace CastIt.Domain.Dtos.Responses
{
    public class ServerPlayerStatusResponseDto
    {
        public PlayerStatusResponseDto Player { get; set; }
        public GetAllPlayListResponseDto PlayList { get; set; }
        public FileItemResponseDto PlayedFile { get; set; }
        public List<FileThumbnailRangeResponseDto> ThumbnailRanges { get; set; } = new List<FileThumbnailRangeResponseDto>();
    }
}
