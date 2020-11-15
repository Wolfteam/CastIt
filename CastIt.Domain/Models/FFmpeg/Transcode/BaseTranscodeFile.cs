﻿namespace CastIt.Domain.Models.FFmpeg.Transcode
{
    public class BaseTranscodeFileBuilder<TBuilder, TFile>
        where TBuilder : class
        where TFile : BaseTranscodeFile, new()
    {
        protected readonly TFile File = new TFile();

        public TBuilder WithFile(string path)
        {
            File.FilePath = path;
            return this as TBuilder;
        }

        public TBuilder GoTo(double seconds)
        {
            File.Seconds = seconds;
            return this as TBuilder;
        }

        public TBuilder ForceTranscode(bool forVideo, bool forAudio)
        {
            File.ForceVideoTranscode = forVideo;
            File.ForceAudioTranscode = forAudio;
            return this as TBuilder;
        }

        public TFile Build() => File;
    }

    public class BaseTranscodeFile
    {
        public string FilePath { get; set; }
        public double Seconds { get; set; }
        public bool ForceVideoTranscode { get; set; }
        public bool ForceAudioTranscode { get; set; }
    }
}
