namespace CastIt.Server.Dtos.Requests
{
    public class SetFileOptionsRequestDto : BaseSocketRequestDto
    {
        public int StreamIndex { get; set; }
        public bool IsAudio { get; set; }
        public bool IsSubTitle { get; set; }
        public bool IsQuality { get; set; }
    }
}
