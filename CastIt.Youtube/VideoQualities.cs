using CastIt.Domain.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace CastIt.Youtube
{
    public class VideoQualities
    {
        public List<VideoQuality> FromFormats { get; set; }
        public List<VideoQuality> FromAdaptiveFormats { get; set; }
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

        public string GetStreamFromFormats(int desiredQuality)
        {
            int closest = FromFormats
                .Select(k => k.Quality)
                .GetClosest(desiredQuality);
            SelectedQuality = closest;
            return FromFormats
                .First(kvp => kvp.Quality == closest)
                .Stream;
        }

        public (string, string) GetStreamsFromAdaptiveFormats(int desiredQuality)
        {
            string audioStream = FromAdaptiveFormats
                .First(q => q.ContainsOnlyAudio)
                .Stream;
            int videoQuality = FromAdaptiveFormats
                .Select(q => q.Quality)
                .GetClosest(desiredQuality);
            string videoStream = FromAdaptiveFormats
                .First(kvp => kvp.Quality == videoQuality)
                .Stream;

            SelectedQuality = videoQuality;
            return (videoStream, audioStream);
        }
    }
}
