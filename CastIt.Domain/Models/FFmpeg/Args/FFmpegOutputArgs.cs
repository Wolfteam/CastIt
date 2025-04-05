using CastIt.Domain.Enums;

namespace CastIt.Domain.Models.FFmpeg.Args
{
    public class FFmpegOutputArgs : FFmpegArgs<FFmpegOutputArgs>
    {
        private readonly string _output;

        public FFmpegOutputArgs(string output)
        {
            _output = output;
        }

        public override string GetArgs()
            => @$"{base.GetArgs()} ""{_output}""";

        public FFmpegOutputArgs SetVideoCodec(string codec)
            => AddArg("c:v", codec);

        public FFmpegOutputArgs CopyVideoCodec()
            => SetVideoCodec("copy");

        public FFmpegOutputArgs SetVideoCodec(HwAccelDeviceType type)
        {
            return type switch
            {
                HwAccelDeviceType.Intel => SetVideoCodec(IntelH264VideoEncoderDecoder),
                HwAccelDeviceType.Nvidia => SetVideoCodec(NvidiaH264VideoEncoder),
                HwAccelDeviceType.AMD => SetVideoCodec(AMDH264VideoEncoder),
                _ => SetVideoCodec("libx264")
            };
        }

        public FFmpegOutputArgs SetAudioCodec(string codec)
            => AddArg("c:a", codec);

        public FFmpegOutputArgs CopyAudioCodec()
            => SetAudioCodec("copy");

        public FFmpegOutputArgs SetFormat(string format)
            => AddArg("f", format);

        public FFmpegOutputArgs SetAudioChannels(int channels)
            => AddArg("ac", channels);

        public FFmpegOutputArgs SetMap(int streamIndex)
            => AddArg("map", $"0:{streamIndex}");

        public FFmpegOutputArgs SetPreset(string preset)
            => AddArg("preset", $"{preset}");

        public FFmpegOutputArgs SetFastPreset()
            => AddArg("preset", "fast");

        public FFmpegOutputArgs SetVeryFastPreset()
            => AddArg("preset", "veryfast");

        public FFmpegOutputArgs SetUltraFastPreset()
            => AddArg("preset", "ultrafast");

        public FFmpegOutputArgs SetPreset(HwAccelDeviceType type)
            => type switch
            {
                HwAccelDeviceType.Intel => SetVeryFastPreset(),
                HwAccelDeviceType.Nvidia => SetFastPreset(),
                HwAccelDeviceType.AMD => SetFastPreset(),
                _ => SetUltraFastPreset()
            };

        public FFmpegOutputArgs SetProfileVideo(string profile)
            => AddArg("profile:v", $"{profile}");

        public FFmpegOutputArgs SetLevel(int level)
            => AddArg("level", $"{level}");

        public FFmpegOutputArgs SetAudioBitrate(int bitrate)
            => AddArg("b:a", $"{bitrate}k");

        public FFmpegOutputArgs SetMovFlagToTheStart()
            => AddArg("movflags", "frag_keyframe+empty_moov+faststart");

        public FFmpegOutputArgs SetFilters(string filters)
            => AddArg("vf", filters);

        //AKA -vframes
        public FFmpegOutputArgs SetVideoFrames(int frames)
            => AddArg("frames:v", frames);

        public FFmpegOutputArgs Seek(double seconds)
            => AddArg("ss", seconds);

        public FFmpegOutputArgs To(double seconds)
            => AddArg("to", seconds);

        public FFmpegOutputArgs SetPixelFormat(string format)
            => AddArg("pix_fmt", format);

        public FFmpegOutputArgs SetPixelFormat(HwAccelDeviceType type)
            => type switch
            {
                HwAccelDeviceType.None => SetPixelFormat("yuv420p"),
                _ => this
            };

        public FFmpegOutputArgs SetPlayList(int index)
            => AddArg("map", $"p:{index}");

        public FFmpegOutputArgs SetDelayInSeconds(double seconds)
            => AddArg("muxdelay", seconds);

        public FFmpegOutputArgs SetDelayInMicroSeconds(int ms)
            => AddArg("maxdelay", ms);

        public FFmpegOutputArgs WithVideoFilter(string filter)
            => AddArg("vf", filter);

        public FFmpegOutputArgs WithVideoFilters(params string[] filters)
            => AddArg("vf", string.Join(",", filters));
    }
}
