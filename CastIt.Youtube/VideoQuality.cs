namespace CastIt.Youtube
{
    public class VideoQuality
    {
        public int Quality { get; set; }
        public bool ContainsVideo { get; set; }
        public bool ContainsAudio { get; set; }
        public bool ContainsVideoAndAudio
            => ContainsVideo && ContainsAudio;
        public bool ContainsOnlyAudio
            => !ContainsVideo && ContainsAudio;
        public bool ContainsOnlyVideo
            => ContainsVideo && !ContainsAudio;
        public string Stream { get; set; }

        private VideoQuality()
        {
        }

        public static VideoQuality VideoAndAudio(string stream, int quality)
        {
            return new VideoQuality
            {
                Stream = stream,
                Quality = quality,
                ContainsAudio = true,
                ContainsVideo = true
            };
        }

        public static VideoQuality OnlyVideo(string stream, int quality)
        {
            return new VideoQuality
            {
                Stream = stream,
                Quality = quality,
                ContainsVideo = true
            };
        }

        public static VideoQuality OnlyAudio(string stream)
        {
            return new VideoQuality
            {
                Quality = -1,
                Stream = stream,
                ContainsAudio = true
            };
        }
    }
}
