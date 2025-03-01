using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace CastIt.Youtube;

public class StreamFormats
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public List<StreamFormat> Formats { get; set; } = [];
    public List<StreamFormat> AdaptiveFormats { get; set; } = [];

    public List<StreamFormat> AllFormats
        => Formats.Concat(AdaptiveFormats).ToList();

    public static StreamFormats GetStreamsFromStreamMap(string streamMap)
    {
        if (string.IsNullOrWhiteSpace(streamMap))
        {
            return new StreamFormats();
        }

        string json = "{" + streamMap + "}";

        return JsonSerializer.Deserialize<StreamFormats>(json, Options);
    }
}

public class StreamFormat
{
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string MimeType { get; set; }
    public string Quality { get; set; }
    public string SignatureCipher { get; set; }
    public string Url { get; set; }

    public bool IsAudio
        => !string.IsNullOrWhiteSpace(MimeType) && MimeType.Contains("video.mp4", StringComparison.OrdinalIgnoreCase);

    public bool IsVideo
        => !string.IsNullOrWhiteSpace(MimeType) && MimeType.Contains("audio.mp4", StringComparison.OrdinalIgnoreCase);
}