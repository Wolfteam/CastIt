namespace CastIt.Domain.Dtos.Responses
{
    public class GetAllPlayListResponseDto
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public int Position { get; set; }

        public bool Loop { get; set; }

        public bool Shuffle { get; set; }

        public long NumberOfFiles { get; set; }
        public string TotalDuration { get; set; }
    }
}
