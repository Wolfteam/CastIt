namespace CastIt.Domain.Dtos.Requests
{
    public class PlayCliFileRequestDto
    {
        public string Mrl { get; set; }
        public int VideoStreamIndex { get; set; }
        public int AudioStreamIndex { get; set; }
        public int SubtitleStreamIndex { get; set; }
        public int Quality { get; set; }
        public int Seconds { get; set; }

        public static PlayCliFileRequestDto FromLocal(string mrl, int videoStreamIndex, int audioStreamIndex, int subsStreamIndex)
        {
            return new PlayCliFileRequestDto
            {
                Mrl = mrl,
                VideoStreamIndex = videoStreamIndex,
                AudioStreamIndex = audioStreamIndex,
                SubtitleStreamIndex = subsStreamIndex,
                Quality = -1,
            };
        }

        public static PlayCliFileRequestDto FromYoutube(string mrl, int quality, int subsStreamIndex = -1)
        {
            return new PlayCliFileRequestDto
            {
                Mrl = mrl,
                Quality = quality,
                SubtitleStreamIndex = subsStreamIndex
            };
        }
    }
}
