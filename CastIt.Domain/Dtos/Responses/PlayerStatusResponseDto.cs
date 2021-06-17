namespace CastIt.Domain.Dtos.Responses
{
    public class PlayerStatusResponseDto
    {
        public string Mrl { get; set; }

        public bool IsPlaying { get; set; }
        public bool IsPaused { get; set; }
        public bool IsPlayingOrPaused { get; set; }

        public double CurrentMediaDuration { get; set; }
        public double ElapsedSeconds { get; set; }
        public double PlayedPercentage { get; set; }

        public double VolumeLevel { get; set; }
        public bool IsMuted { get; set; }
    }
}
