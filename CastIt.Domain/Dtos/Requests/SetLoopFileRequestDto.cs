namespace CastIt.Domain.Dtos.Requests
{
    public class SetLoopFileRequestDto : BaseSocketRequestDto
    {
        public long Id { get; set; }
        public long PlayListId { get; set; }
        public bool Loop { get; set; }
    }
}
