namespace CastIt.Domain.Dtos.Responses
{
    public class FileItemResponseDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double TotalSeconds { get; set; }
        public string Path { get; set; }
        public int Position { get; set; }
        public double PlayedPercentage { get; set; }
        public long PlayListId { get; set; }

        public bool IsBeingPlayed { get; set; }
        public bool IsLocalFile { get; set; }
        public bool IsUrlFile { get; set; }
        public double PlayedSeconds { get; set; }
        public bool CanStartPlayingFromCurrentPercentage { get; set; }
        public bool WasPlayed { get; set; }
        public bool IsCached { get; set; }

        public bool Exists { get; set; }
        public string Filename { get; set; }
        public string Size { get; set; }
        public string Extension { get; set; }
        public bool Loop { get; set; }
        public string SubTitle { get; set; }
        public string Resolution { get; set; }

        public string Duration { get; set; }
        public string PlayedTime { get; set; }
        public string TotalDuration { get; set; }
    }
}
