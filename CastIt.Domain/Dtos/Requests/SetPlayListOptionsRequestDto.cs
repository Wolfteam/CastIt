namespace CastIt.Domain.Dtos.Requests
{
    public class SetPlayListOptionsRequestDto : BaseSocketRequestDto
    {
        public long Id { get; set; }
        public bool Loop { get; set; }
        public bool Shuffle { get; set; }
    }
}
