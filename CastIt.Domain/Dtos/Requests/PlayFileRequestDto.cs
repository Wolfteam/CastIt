namespace CastIt.Domain.Dtos.Requests
{
    public class PlayFileRequestDto
    {
        public long Id { get; set; }
        public long PlayListId { get; set; }
        public bool Force { get; set; }
        public bool FileOptionsChanged { get; set; }
    }
}
