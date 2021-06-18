namespace CastIt.Domain.Dtos.Requests
{
    public class SetLoopFileRequestDto
    {
        public long Id { get; set; }
        public long PlayListId { get; set; }
        public bool Loop { get; set; }
    }
}
