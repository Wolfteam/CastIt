namespace CastIt.Youtube;

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

    public StreamFormat StreamFormat { get; set; }

    private VideoQuality()
    {
    }

    public static VideoQuality VideoAndAudio(StreamFormat streamFormat, int quality)
    {
        return new VideoQuality
        {
            StreamFormat = streamFormat,
            Quality = quality,
            ContainsAudio = true,
            ContainsVideo = true
        };
    }

    public static VideoQuality OnlyVideo(StreamFormat streamFormat, int quality)
    {
        return new VideoQuality
        {
            StreamFormat = streamFormat,
            Quality = quality,
            ContainsVideo = true
        };
    }

    public static VideoQuality OnlyAudio(StreamFormat streamFormat)
    {
        return new VideoQuality
        {
            Quality = -1,
            StreamFormat = streamFormat,
            ContainsAudio = true
        };
    }
}