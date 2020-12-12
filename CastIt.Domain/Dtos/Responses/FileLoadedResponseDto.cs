namespace CastIt.Domain.Dtos.Responses
{
    public class FileLoadedResponseDto
    {
        public long Id { get; set; }
        public string Filename { get; set; }
        public string ThumbnailUrl { get; set; }
        public double Duration { get; set; }
        public bool LoopFile { get; set; }
        public double CurrentSeconds { get; set; }
        public bool IsPaused { get; set; }

        public double VolumeLevel { get; set; }
        public bool IsMuted { get; set; }

        public long PlayListId { get; set; }
        public string PlayListName { get; set; }
        public bool LoopPlayList { get; set; }
        public bool ShufflePlayList { get; set; }
    }
}
