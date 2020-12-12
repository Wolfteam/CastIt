namespace CastIt.Domain.Dtos.Requests
{
    public class DeleteFileRequestDto : BaseSocketRequestDto
    {
        public long Id { get; set; }
        public long PlayListId { get; set; }
    }
}
