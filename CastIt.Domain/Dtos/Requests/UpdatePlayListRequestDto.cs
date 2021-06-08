namespace CastIt.Domain.Dtos.Requests
{
    public class UpdatePlayListRequestDto : BaseItemRequestDto
    {
        public string Name { get; set; }
        public int? Position { get; set; }
    }
}
