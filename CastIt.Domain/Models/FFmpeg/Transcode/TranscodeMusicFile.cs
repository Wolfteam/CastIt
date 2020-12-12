namespace CastIt.Domain.Models.FFmpeg.Transcode
{
    public class TranscodeMusicFileBuilder : BaseTranscodeFileBuilder<TranscodeMusicFileBuilder, TranscodeMusicFile>
    {
        public TranscodeMusicFileBuilder WithAudio(int streamIndex)
        {
            File.AudioStreamIndex = streamIndex;
            return this;
        }
    }

    public class TranscodeMusicFile : BaseTranscodeFile
    {
        public int AudioStreamIndex { get; set; }
    }
}
