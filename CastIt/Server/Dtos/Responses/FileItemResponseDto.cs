namespace CastIt.Server.Dtos.Responses
{
    public class FileItemResponseDto
    {
        public long Id { get; set; }
        public int Position { get; set; }
        public string Path { get; set; }
        public double PlayedSeconds { get; set; }
        public double TotalSeconds { get; set; }
        public double PlayedPercentage { get; set; }
        public bool IsBeingPlayed { get; set; }
        public long PlayListId { get; set; }
        public bool IsLocalFile { get; set; }
        public bool IsUrlFile { get; set; }
        public bool Exists { get; set; }
        public string Filename { get; set; }
        public string Size { get; set; }
        public string Extension { get; set; }
        public bool Loop { get; set; }
        public string SubTitle { get; set; }
    }
}
