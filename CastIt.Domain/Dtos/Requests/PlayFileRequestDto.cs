namespace CastIt.Domain.Dtos.Requests
{
    public class PlayFileRequestDto : BaseSocketRequestDto
    {
        public long Id { get; set; }
        public long PlayListId { get; set; }
        public bool Force { get; set; }
    }
}
