namespace CastIt.Domain.Dtos.Requests
{
    public class SetVolumeRequestDto : BaseSocketRequestDto
    {
        public double VolumeLevel { get; set; }
        public bool IsMuted { get; set; }
    }
}