using System.Collections.Generic;

namespace CastIt.Domain.Models.FFmpeg.Args
{
    public abstract class FFmpegArgs
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
    }
}
