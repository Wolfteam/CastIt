namespace CastIt.Server.Dtos.Requests
{
    public class GoToRequestDto : BaseSocketRequestDto
    {
        public bool Previous { get; set; }
        public bool Next { get; set; }
    }
}
