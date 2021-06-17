namespace CastIt.Domain.Dtos.Requests
{
    public class PlayFileRequestDto : BaseSocketRequestDto
    {
        public long Id { get; set; }
        public bool Force { get; set; }
        public bool FileOptionsChanged { get; set; }
    }
}
