using CastIt.Common.Enums;

namespace CastIt.Models.FFMpeg.Args
{
    public class FFmpegInputArgs : FFmpegArgs
    {
        private readonly string _input;

        public FFmpegInputArgs(string Input)
        {
            _input = Input;
        }

        public override string GetArgs()
            => @$"{base.GetArgs()} -i ""{_input}""";

        public FFmpegInputArgs AddArg(string Arg)
        {
            Args.Add($"-{Arg}");

            return this;
        }

        public FFmpegInputArgs AddArg<T>(string key, T value)
            => AddArg($"{key} {value}");

        public FFmpegInputArgs SetHwAccel(string type)
            => AddArg("hwaccel", type);

        public FFmpegInputArgs SetHwAccel(HwAccelDeviceType type)
        {
            return type switch
            {
                HwAccelDeviceType.Intel => SetHwAccel(IntelHwAccel),
                HwAccelDeviceType.Nvidia => SetHwAccel(NvidiaHwAccel),
                HwAccelDeviceType.AMD => SetHwAccel(AMDHwAccel),
                _ => this,
            };
        }

        public FFmpegInputArgs SetVideoCodec(string codec)
            => AddArg("c:v", codec);

        public FFmpegInputArgs SetVideoCodec(HwAccelDeviceType type)
        {
            return type switch
            {
                HwAccelDeviceType.Intel => SetVideoCodec(IntelH264VideoEncoderDecoder),
                HwAccelDeviceType.Nvidia => SetVideoCodec(NvidiaH264VideoDecoder),
                HwAccelDeviceType.AMD => SetVideoCodec(AMDH264VideoEncoder),
                _ => this,
            };
        }

        public FFmpegInputArgs DisableVideo()
            => AddArg("vn");

        public FFmpegInputArgs DisableAudio()
            => AddArg("an");

        public FFmpegInputArgs BeQuiet()
            => AddArg("v", "quiet");

        public FFmpegInputArgs SetAutoConfirmChanges()
            => AddArg("y");

        public FFmpegInputArgs Seek(double seconds)
            => AddArg("ss", seconds);

        public FFmpegInputArgs SetVSync(int value)
            => AddArg("vsync", value);

        public FFmpegInputArgs Discard(string input)
            => AddArg("discard", input);
    }
}
