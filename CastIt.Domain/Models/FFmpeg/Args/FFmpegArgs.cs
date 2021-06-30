using System.Collections.Generic;
using System.Globalization;

namespace CastIt.Domain.Models.FFmpeg.Args
{
    public abstract class FFmpegArgs<TArgs> where TArgs : class
    {
        public const string NvidiaHwAccel = "cuvid";
        public const string NvidiaH264VideoDecoder = "h264_cuvid";
        public const string NvidiaH264VideoEncoder = "h264_nvenc";

        public const string IntelHwAccel = "qsv";
        public const string IntelH264VideoEncoderDecoder = "h264_qsv";
        public const string IntelImgEncoder = "mjpeg_qsv";

        //TODO: AMD IS NOT TESTED
        public const string AMDHwAccel = "amf";
        public const string AMDH264VideoEncoder = "h264_amf";

        protected readonly List<string> Args = new List<string>();

        public virtual string GetArgs()
        {
            return string.Join(" ", Args);
        }

        public TArgs AddArg(string arg)
        {
            var newArg = $"-{arg}";
            if (!Args.Contains(newArg))
            {
                Args.Add(newArg);
            }
            return this as TArgs;
        }

        public TArgs AddArg<T>(string key, T value)
        {
            //This is to avoid something like 10,21 instead of 10.21
            var arg = value switch
            {
                double v => v.ToString(CultureInfo.InvariantCulture),
                float v => v.ToString(CultureInfo.InvariantCulture),
                _ => $"{value}"
            };

            return AddArg($"{key} {arg}");
        }
    }
}
