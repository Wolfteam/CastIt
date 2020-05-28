namespace CastIt.GoogleCast.Models.Media
{
    public class MediaStatus
    {
        public long MediaSessionId { get; set; }
        public int PlaybackRate { get; set; }
        public string PlayerState { get; set; }
        public double CurrentTime { get; set; }
        public int SupportedMediaCommands { get; set; }
        public Volume Volume { get; set; }
        public string IdleReason { get; set; }
        public MediaInformation Media { get; set; }
        public int CurrentItemId { get; set; }
        public MediaStatus ExtendedStatus { get; set; }
        public string RepeatMode { get; set; }
    }
}
