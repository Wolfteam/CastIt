using CastIt.Domain.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace CastIt.Youtube;

public class VideoQualities
{
    public List<VideoQuality> FromFormats { get; }
    public List<VideoQuality> FromAdaptiveFormats { get; }
    public int SelectedQuality { get; private set; }

    public bool UseAdaptiveFormats =>
        FromAdaptiveFormats.Any(a => a.ContainsOnlyAudio) &&
        FromAdaptiveFormats.Any(a => a.ContainsOnlyVideo);

    internal VideoQualities(
        List<VideoQuality> fromFormats,
        List<VideoQuality> fromAdaptiveFormats)
    {
        FromFormats = fromFormats;
        FromAdaptiveFormats = fromAdaptiveFormats;
    }

    public StreamFormat GetStreamFromFormats(int desiredQuality)
    {
        int closest = FromFormats
            .Select(k => k.Quality)
            .GetClosest(desiredQuality);
        SelectedQuality = closest;
        return FromFormats
            .First(kvp => kvp.Quality == closest)
            .StreamFormat;
    }

    public (StreamFormat, StreamFormat) GetStreamsFromAdaptiveFormats(int desiredQuality)
    {
        StreamFormat audioStream = FromAdaptiveFormats
            .First(q => q.ContainsOnlyAudio)
            .StreamFormat;
        int videoQuality = FromAdaptiveFormats
            .Select(q => q.Quality)
            .GetClosest(desiredQuality);
        StreamFormat videoStream = FromAdaptiveFormats
            .First(kvp => kvp.Quality == videoQuality)
            .StreamFormat;

        SelectedQuality = videoQuality;
        return (videoStream, audioStream);
    }
}