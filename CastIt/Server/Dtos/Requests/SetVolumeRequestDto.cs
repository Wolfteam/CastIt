namespace CastIt.Server.Dtos.Requests
{
    public class SetVolumeRequestDto : BaseSocketRequestDto
    {
        public double VolumeLevel { get; set; }
        public bool IsMuted { get; set; }
    }
}