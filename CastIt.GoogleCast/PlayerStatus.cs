using CastIt.GoogleCast.Interfaces;

namespace CastIt.GoogleCast
{
    public class PlayerStatus
    {
        public string Mrl { get; }

        public bool IsPlaying { get; }
        public bool IsPaused { get; }
        public bool IsPlayingOrPaused { get; }

        public double CurrentMediaDuration { get; }
        public double ElapsedSeconds { get; }
        public double PlayedPercentage { get; }

        public double VolumeLevel { get; }
        public bool IsMuted { get; }

        public PlayerStatus(IPlayer player)
        {
            Mrl = player.CurrentContentId;
            IsPlaying = player.IsPlaying;
            IsPaused = player.IsPaused;
            IsPlayingOrPaused = player.IsPlayingOrPaused;
            CurrentMediaDuration = player.CurrentMediaDuration;
            ElapsedSeconds = player.ElapsedSeconds;
            PlayedPercentage = player.PlayedPercentage;
            VolumeLevel = player.CurrentVolumeLevel;
            IsMuted = player.IsMuted;
        }
    }
}
