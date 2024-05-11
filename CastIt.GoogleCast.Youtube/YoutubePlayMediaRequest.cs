using CastIt.GoogleCast.Models.Play;
using System.Collections.Generic;

namespace CastIt.GoogleCast.Youtube;

public class YoutubePlayMediaRequest : PlayMediaRequest
{
    public int VideoQuality { get; set; }
    public int SelectedQuality { get; set; }
    public List<int> Qualities { get; set; }
    public bool UsesAdaptiveFormats { get; set; }
}