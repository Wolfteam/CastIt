using CastIt.Domain.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CastIt.Domain.Models.FFmpeg.Args
{
    public class FFmpegInputArgs : FFmpegArgs
    {
        private readonly string _input;

        public FFmpegInputArgs(string input)
        {
            _input = input;
        }

        public override string GetArgs()
            => @$"{base.GetArgs()} -i ""{_input}""";

        public FFmpegInputArgs AddArg(string arg)
        {
            Args.Add($"-{arg}");

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

        //https://trac.ffmpeg.org/ticket/2431
        public FFmpegInputArgs TrySetSubTitleEncoding(IReadOnlyList<string> allowedSubFormats)
        {
            string ext = System.IO.Path.GetExtension(_input);
            if (ext == null)
                return null;
            if (!allowedSubFormats.Contains(ext.ToLower(), StringComparer.OrdinalIgnoreCase))
                return this;
            var encoding = GetEncoding(_input);
            return encoding == null ? this : AddArg("sub_charenc", encoding.BodyName);
        }

        private static Encoding GetEncoding(string filePath)
        {
            var encodingByBOM = GetEncodingByBOM(filePath);
            if (encodingByBOM != null)
                return encodingByBOM;

            // BOM not found :(, so try to parse characters into several encodings
            var encodingByParsingUTF8 = GetEncodingByParsing(filePath, Encoding.UTF8);
            if (encodingByParsingUTF8 != null)
                return encodingByParsingUTF8;

            var encodingByParsingLatin1 = GetEncodingByParsing(filePath, Encoding.GetEncoding("iso-8859-1"));
            if (encodingByParsingLatin1 != null)
                return encodingByParsingLatin1;
#pragma warning disable SYSLIB0001 // Type or member is obsolete
            var encodingByParsingUTF7 = GetEncodingByParsing(filePath, Encoding.UTF7);
#pragma warning restore SYSLIB0001 // Type or member is obsolete
            return encodingByParsingUTF7;
        }

        private static Encoding GetEncodingByBOM(string filePath)
        {
            // Read the BOM
            var byteOrderMark = new byte[4];
            using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                file.Read(byteOrderMark, 0, 4);
            }

            // Analyze the BOM
            if (byteOrderMark[0] == 0x2b && byteOrderMark[1] == 0x2f && byteOrderMark[2] == 0x76)
#pragma warning disable SYSLIB0001 // Type or member is obsolete
                return Encoding.UTF7;
#pragma warning restore SYSLIB0001 // Type or member is obsolete
            if (byteOrderMark[0] == 0xef && byteOrderMark[1] == 0xbb && byteOrderMark[2] == 0xbf)
                return Encoding.UTF8;
            if (byteOrderMark[0] == 0xff && byteOrderMark[1] == 0xfe)
                return Encoding.Unicode; //UTF-16LE
            if (byteOrderMark[0] == 0xfe && byteOrderMark[1] == 0xff)
                return Encoding.BigEndianUnicode; //UTF-16BE
            if (byteOrderMark[0] == 0 && byteOrderMark[1] == 0 && byteOrderMark[2] == 0xfe && byteOrderMark[3] == 0xff)
                return Encoding.UTF32;

            return null;    // no BOM found
        }

        private static Encoding GetEncodingByParsing(string filePath, Encoding encoding)
        {
            var encodingVerifier = Encoding.GetEncoding(encoding.BodyName, new EncoderExceptionFallback(), new DecoderExceptionFallback());

            try
            {
                using var textReader = new StreamReader(filePath, encodingVerifier, true);
                while (!textReader.EndOfStream)
                {
                    textReader.ReadLine();   // in order to increment the stream position
                }

                // all text parsed ok
                return textReader.CurrentEncoding;
            }
            catch
            {
                // ignored
            }

            return null;
        }
    }
}
