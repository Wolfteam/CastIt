namespace CastIt.Domain.Dtos.Responses
{
    public class ServerPlayerStatusResponseDto
    {
        public PlayerStatusResponseDto Player { get; set; }
        public GetAllPlayListResponseDto PlayList { get; set; }
        public FileItemResponseDto PlayedFile { get; set; }
    }
}
